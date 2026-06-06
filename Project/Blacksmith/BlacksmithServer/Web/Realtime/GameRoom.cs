using BlacksmithCore.Driver;
using BlacksmithCore.Infra.Models.Components;
using BlacksmithCore.Infra.Models.Entites;
using ClapInfra.ClapModels.Components;

namespace BlacksmithServer.Web.Realtime
{
    public sealed class GameRoom
    {
        private const int RoundTimeoutSeconds = 30;
        private const int TimeoutLossThreshold = 3;

        private readonly SemaphoreSlim _gate = new(1, 1);
        private readonly GameInstance _game = new();
        private readonly List<TurnLog> _turns = new();
        private readonly Func<string, BattleSnapshot, string?, Task> _persistAndSendAsync;
        private readonly Func<GameRoom, Task> _onCompletedAsync;

        private PendingTurn? _playerOnePending;
        private PendingTurn? _playerTwoPending;
        private int _playerOneTimeouts;
        private int _playerTwoTimeouts;
        private DateTimeOffset? _roundDeadlineUtc;
        private CancellationTokenSource? _roundTimerCts;
        private bool _completed;
        private MatchCompletion? _completion;

        public string PlayerOneUsername { get; }
        public string PlayerTwoUsername { get; }

        public GameRoom(
            string playerOneUsername,
            string playerTwoUsername,
            Func<string, BattleSnapshot, string?, Task> persistAndSendAsync,
            Func<GameRoom, Task> onCompletedAsync)
        {
            PlayerOneUsername = playerOneUsername;
            PlayerTwoUsername = playerTwoUsername;
            _persistAndSendAsync = persistAndSendAsync;
            _onCompletedAsync = onCompletedAsync;
        }

        public async Task StartAsync()
        {
            Dictionary<string, BattleSnapshot> snapshots;

            await _gate.WaitAsync();
            try
            {
                StartNextRoundNoLock();
                snapshots = BuildSnapshotsNoLock();
            }
            finally
            {
                _gate.Release();
            }

            await PushSnapshotsAsync(snapshots, $"Match found. Submit your skill within {RoundTimeoutSeconds} seconds.");
        }

        public BattleSnapshot BuildSnapshot(string username)
        {
            var participant = ResolveParticipant(username);
            _gate.Wait();
            try
            {
                return participant == RoomParticipant.PlayerOne
                    ? BuildSnapshotNoLock(RoomParticipant.PlayerOne)
                    : BuildSnapshotNoLock(RoomParticipant.PlayerTwo);
            }
            finally
            {
                _gate.Release();
            }
        }

        public async Task SubmitTurnAsync(string username, string skillName, int param, string stringParam = "")
        {
            var normalizedSkill = NormalizeSkillName(skillName);
            var participant = ResolveParticipant(username);
            var shouldResolve = false;
            var message = string.Empty;
            Dictionary<string, BattleSnapshot>? snapshots = null;

            await _gate.WaitAsync();
            try
            {
                if (_completed)
                {
                    snapshots = BuildSnapshotsNoLock();
                    message = "The match has already ended.";
                }
                else if (_roundDeadlineUtc == null)
                {
                    snapshots = BuildSnapshotsNoLock();
                    message = "The round is not accepting inputs right now.";
                }
                else if (GetPendingNoLock(participant) != null)
                {
                    snapshots = BuildSnapshotsNoLock();
                    message = "You already locked in a skill for this round.";
                }
                else
                {
                    var validation = participant == RoomParticipant.PlayerOne
                        ? _game.TryDeclare(normalizedSkill, param, stringParam)
                        : _game.ETryDeclare(normalizedSkill, param, stringParam);

                    Console.WriteLine($"[GameRoom] {username}: TryDeclare('{normalizedSkill}', param={param}, stringParam='{stringParam}') => {validation}");

                    if (validation != SkillDeclareResult.Success)
                    {
                        var community = participant == RoomParticipant.PlayerOne ? _game.Player : _game.Enemy;
                        var resources = community.Focus.Get<Resource>().GetView();
                        var resSummary = string.Join(", ", resources.Select(r => $"{r.name}={r.quantity}"));
                        Console.WriteLine($"[GameRoom] {username}: validation failed. Available resources: [{resSummary}]");
                        snapshots = BuildSnapshotsNoLock();
                        message = $"Skill '{normalizedSkill}' {validation}.";
                    }
                    else
                    {
                        SetPendingNoLock(participant, new PendingTurn(normalizedSkill, param, stringParam, false));
                        shouldResolve = _playerOnePending != null && _playerTwoPending != null;

                        if (shouldResolve)
                        {
                            CancelRoundTimerNoLock();
                        }
                        else
                        {
                            snapshots = BuildSnapshotsNoLock();
                            message = "Turn accepted. Waiting for the other player.";
                        }
                    }
                }
            }
            finally
            {
                _gate.Release();
            }

            if (shouldResolve)
            {
                await ResolveRoundAsync("Both players locked in their turns.");
                return;
            }

            if (snapshots != null)
            {
                await PushSnapshotsAsync(snapshots, message);
            }
        }

        public async Task HandleDisconnectAsync(string username)
        {
            Dictionary<string, BattleSnapshot>? snapshots = null;
            var message = string.Empty;
            var shouldComplete = false;

            await _gate.WaitAsync();
            try
            {
                if (_completed)
                {
                    return;
                }

                var disconnectedParticipant = ResolveParticipant(username);
                CancelRoundTimerNoLock();
                _roundDeadlineUtc = null;
                _completed = true;
                _completion = disconnectedParticipant == RoomParticipant.PlayerOne
                    ? new MatchCompletion("lose", "win", MatchEndReason.OpponentDisconnected)
                    : new MatchCompletion("win", "lose", MatchEndReason.OpponentDisconnected);

                snapshots = BuildSnapshotsNoLock();
                message = disconnectedParticipant == RoomParticipant.PlayerOne
                    ? $"{PlayerOneUsername} disconnected."
                    : $"{PlayerTwoUsername} disconnected.";
                shouldComplete = true;
            }
            finally
            {
                _gate.Release();
            }

            if (snapshots != null)
            {
                await PushSnapshotsAsync(snapshots, message);
            }

            if (shouldComplete)
            {
                await _onCompletedAsync(this);
            }
        }

        private async Task ResolveRoundAsync(string message)
        {
            Dictionary<string, BattleSnapshot> snapshots;
            var shouldComplete = false;

            await _gate.WaitAsync();
            try
            {
                if (_completed || _roundDeadlineUtc == null)
                {
                    return;
                }

                _roundDeadlineUtc = null;

                var playerOneTurn = _playerOnePending ?? new PendingTurn("iron", 0, "", true);
                var playerTwoTurn = _playerTwoPending ?? new PendingTurn("iron", 0, "", true);

                _playerOnePending = null;
                _playerTwoPending = null;

                _game.Declare(playerOneTurn.SkillName, playerOneTurn.Param, playerTwoTurn.SkillName, playerTwoTurn.Param, playerOneTurn.StringParam, playerTwoTurn.StringParam);

                if (playerOneTurn.TimedOut)
                {
                    _playerOneTimeouts++;
                }

                if (playerTwoTurn.TimedOut)
                {
                    _playerTwoTimeouts++;
                }

                _turns.Add(new TurnLog(
                    _turns.Count + 1,
                    playerOneTurn.SkillName,
                    playerOneTurn.Param,
                    playerOneTurn.StringParam,
                    playerTwoTurn.SkillName,
                    playerTwoTurn.Param,
                    playerTwoTurn.StringParam,
                    playerOneTurn.TimedOut,
                    playerTwoTurn.TimedOut,
                    BuildTurnNote(playerOneTurn.TimedOut, playerTwoTurn.TimedOut)));

                _completion = EvaluateCompletionNoLock(playerOneTurn.TimedOut, playerTwoTurn.TimedOut);
                if (_completion != null)
                {
                    _completed = true;
                    CancelRoundTimerNoLock();
                    shouldComplete = true;
                }
                else
                {
                    StartNextRoundNoLock();
                }

                snapshots = BuildSnapshotsNoLock();
            }
            finally
            {
                _gate.Release();
            }

            await PushSnapshotsAsync(snapshots, message);

            if (shouldComplete)
            {
                await _onCompletedAsync(this);
            }
        }

        private MatchCompletion? EvaluateCompletionNoLock(bool playerOneTimedOut, bool playerTwoTimedOut)
        {
            if (_playerOneTimeouts >= TimeoutLossThreshold || _playerTwoTimeouts >= TimeoutLossThreshold)
            {
                if (_playerOneTimeouts >= TimeoutLossThreshold && _playerTwoTimeouts >= TimeoutLossThreshold && playerOneTimedOut && playerTwoTimedOut)
                {
                    return new MatchCompletion("draw", "draw", MatchEndReason.SimultaneousTimeoutDraw);
                }

                if (_playerOneTimeouts >= TimeoutLossThreshold)
                {
                    return new MatchCompletion("lose", "win", MatchEndReason.TimeoutForfeit);
                }

                if (_playerTwoTimeouts >= TimeoutLossThreshold)
                {
                    return new MatchCompletion("win", "lose", MatchEndReason.TimeoutForfeit);
                }
            }

            var playerView = _game.Player.Focus.GetView();
            var enemyView = _game.Enemy.Focus.GetView();
            var playerDead = playerView.HP <= 0;
            var enemyDead = enemyView.HP <= 0;

            if (playerDead && enemyDead)
            {
                return new MatchCompletion("draw", "draw", MatchEndReason.NormalDefeat);
            }

            if (playerDead)
            {
                return new MatchCompletion("lose", "win", MatchEndReason.NormalDefeat);
            }

            if (enemyDead)
            {
                return new MatchCompletion("win", "lose", MatchEndReason.NormalDefeat);
            }

            return null;
        }

        private Dictionary<string, BattleSnapshot> BuildSnapshotsNoLock()
        {
            return new Dictionary<string, BattleSnapshot>(StringComparer.OrdinalIgnoreCase)
            {
                [PlayerOneUsername] = BuildSnapshotNoLock(RoomParticipant.PlayerOne),
                [PlayerTwoUsername] = BuildSnapshotNoLock(RoomParticipant.PlayerTwo)
            };
        }

        private BattleSnapshot BuildSnapshotNoLock(RoomParticipant participant)
        {
            var viewerIsPlayerOne = participant == RoomParticipant.PlayerOne;
            var selfCommunity = viewerIsPlayerOne ? _game.Player : _game.Enemy;
            var enemyCommunity = viewerIsPlayerOne ? _game.Enemy : _game.Player;
            var selfView = selfCommunity.Focus.GetView();
            var enemyView = enemyCommunity.Focus.GetView();
            var viewerName = viewerIsPlayerOne ? PlayerOneUsername : PlayerTwoUsername;
            var opponentName = viewerIsPlayerOne ? PlayerTwoUsername : PlayerOneUsername;
            var playerTimeouts = viewerIsPlayerOne ? _playerOneTimeouts : _playerTwoTimeouts;
            var enemyTimeouts = viewerIsPlayerOne ? _playerTwoTimeouts : _playerOneTimeouts;
            var hasSubmittedTurn = viewerIsPlayerOne ? _playerOnePending != null : _playerTwoPending != null;
            var result = _completion == null
                ? "next"
                : viewerIsPlayerOne ? _completion.PlayerOneOutcome : _completion.PlayerTwoOutcome;

            return new BattleSnapshot
            {
                Authenticated = true,
                Username = viewerName,
                Status = _completed ? "finished" : "playing",
                Started = true,
                ManualMode = false,
                ModeName = "Online Match",
                Result = result,
                ResultDetail = BuildResultDetailNoLock(participant),
                OpponentName = opponentName,
                Queued = false,
                RoundDeadlineUtc = _roundDeadlineUtc?.ToString("O"),
                HasSubmittedTurn = hasSubmittedTurn,
                PlayerTimeouts = playerTimeouts,
                EnemyTimeouts = enemyTimeouts,
                StatusMessage = BuildStatusMessageNoLock(participant),
                Player = BuildActor(selfView, selfCommunity.Focus.Get<Skill>().GetAvailableSkillNames(), selfCommunity),
                Enemy = BuildActor(enemyView, enemyCommunity.Focus.Get<Skill>().GetAvailableSkillNames(), enemyCommunity),
                Turns = BuildTurnsNoLock(participant)
            };
        }

        private ResultDetailDto? BuildResultDetailNoLock(RoomParticipant participant)
        {
            if (_completion == null)
            {
                return null;
            }

            var outcome = participant == RoomParticipant.PlayerOne ? _completion.PlayerOneOutcome : _completion.PlayerTwoOutcome;
            var title = outcome switch
            {
                "win" => "Win",
                "lose" => "Lose",
                "draw" => "Draw",
                _ => "In Progress"
            };

            var summary = _completion.Reason switch
            {
                MatchEndReason.NormalDefeat when outcome == "win" => "You defeated your opponent through normal combat.",
                MatchEndReason.NormalDefeat when outcome == "lose" => "You were defeated through normal combat.",
                MatchEndReason.NormalDefeat => "Both players were defeated in the same round.",
                MatchEndReason.OpponentDisconnected when outcome == "win" => "Your opponent disconnected and forfeited the match.",
                MatchEndReason.OpponentDisconnected when outcome == "lose" => "You disconnected and forfeited the match.",
                MatchEndReason.TimeoutForfeit when outcome == "win" => "Your opponent reached 3 timeouts and lost the match.",
                MatchEndReason.TimeoutForfeit when outcome == "lose" => "You reached 3 timeouts and lost the match.",
                MatchEndReason.SimultaneousTimeoutDraw => "Both players reached 3 timeouts in the same round.",
                _ => "The match has ended."
            };

            return new ResultDetailDto
            {
                Title = title,
                ReasonCode = ToReasonCode(_completion.Reason),
                Summary = summary
            };
        }

        private string BuildStatusMessageNoLock(RoomParticipant participant)
        {
            if (_completion != null)
            {
                return BuildResultDetailNoLock(participant)?.Summary ?? "The match has ended.";
            }

            if (_roundDeadlineUtc == null)
            {
                return "Resolving the round.";
            }

            if (GetPendingNoLock(participant) != null)
            {
                return "Your turn is locked in. Waiting for the opponent or the timer.";
            }

            return "Submit your skill before the round timer reaches zero.";
        }

        private List<TurnSnapshot> BuildTurnsNoLock(RoomParticipant participant)
        {
            return _turns.Select(turn =>
            {
                if (participant == RoomParticipant.PlayerOne)
                {
                    return new TurnSnapshot
                    {
                        Index = turn.Index,
                        Result = "Continue",
                        PlayerSkill = turn.PlayerOneSkill,
                        PlayerParam = turn.PlayerOneParam,
                        PlayerStringParam = turn.PlayerOneStringParam,
                        EnemySkill = turn.PlayerTwoSkill,
                        EnemyParam = turn.PlayerTwoParam,
                        EnemyStringParam = turn.PlayerTwoStringParam,
                        PlayerTimedOut = turn.PlayerOneTimedOut,
                        EnemyTimedOut = turn.PlayerTwoTimedOut,
                        Note = turn.Note
                    };
                }

                return new TurnSnapshot
                {
                    Index = turn.Index,
                    Result = "Continue",
                    PlayerSkill = turn.PlayerTwoSkill,
                    PlayerParam = turn.PlayerTwoParam,
                    PlayerStringParam = turn.PlayerTwoStringParam,
                    EnemySkill = turn.PlayerOneSkill,
                    EnemyParam = turn.PlayerOneParam,
                    EnemyStringParam = turn.PlayerOneStringParam,
                    PlayerTimedOut = turn.PlayerTwoTimedOut,
                    EnemyTimedOut = turn.PlayerOneTimedOut,
                    Note = turn.Note
                };
            }).ToList();
        }

        private void StartNextRoundNoLock()
        {
            CancelRoundTimerNoLock();
            _roundDeadlineUtc = DateTimeOffset.UtcNow.AddSeconds(RoundTimeoutSeconds);
            _roundTimerCts = new CancellationTokenSource();
            _ = RunRoundTimerAsync(_roundTimerCts.Token);
        }

        private async Task RunRoundTimerAsync(CancellationToken cancellationToken)
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(RoundTimeoutSeconds), cancellationToken);
            }
            catch (OperationCanceledException)
            {
                return;
            }

            try
            {
                await ResolveRoundAsync("Round timer expired. Missing inputs defaulted to iron 0.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GameRoom] Round timer resolution failed: {ex}");
            }
        }

        private void CancelRoundTimerNoLock()
        {
            if (_roundTimerCts == null)
            {
                return;
            }

            _roundTimerCts.Cancel();
            _roundTimerCts.Dispose();
            _roundTimerCts = null;
        }

        private PendingTurn? GetPendingNoLock(RoomParticipant participant)
        {
            return participant == RoomParticipant.PlayerOne ? _playerOnePending : _playerTwoPending;
        }

        private void SetPendingNoLock(RoomParticipant participant, PendingTurn value)
        {
            if (participant == RoomParticipant.PlayerOne)
            {
                _playerOnePending = value;
            }
            else
            {
                _playerTwoPending = value;
            }
        }

        private RoomParticipant ResolveParticipant(string username)
        {
            if (string.Equals(username, PlayerOneUsername, StringComparison.OrdinalIgnoreCase))
            {
                return RoomParticipant.PlayerOne;
            }

            if (string.Equals(username, PlayerTwoUsername, StringComparison.OrdinalIgnoreCase))
            {
                return RoomParticipant.PlayerTwo;
            }

            throw new InvalidOperationException($"User '{username}' is not part of this room.");
        }

        private async Task PushSnapshotsAsync(Dictionary<string, BattleSnapshot> snapshots, string? message)
        {
            await _persistAndSendAsync(PlayerOneUsername, snapshots[PlayerOneUsername], message);
            await _persistAndSendAsync(PlayerTwoUsername, snapshots[PlayerTwoUsername], message);
        }

        private static ActorSnapshot BuildActor(BodyView view, List<string> availableSkills, Community? community = null)
        {
            return new ActorSnapshot
            {
                BodyName = view.BodyName,
                Professions = view.ProfessionNames,
                Hp = view.HP,
                MaxHp = view.MHP,
                Defenses = view.DefenseView.Select(d => new NamedValueSnapshot
                {
                    Name = d.name,
                    Power = d.power
                }).ToList(),
                Resources = view.ResourcesView.Select(r => new ResourceSnapshot
                {
                    Name = r.name,
                    Quantity = r.quantity
                }).ToList(),
                FutureAttacks = view.FutureAttackView.Select(f => new FutureValueSnapshot
                {
                    Name = f.name,
                    DelayRounds = f.delayRounds,
                    Power = f.power
                }).ToList(),
                FutureDefenses = view.FutureDefenseView.Select(f => new FutureValueSnapshot
                {
                    Name = f.name,
                    DelayRounds = f.delayRounds,
                    Power = f.power
                }).ToList(),
                AvailableSkills = availableSkills,
                Summons = community?.SummonList.Select(s =>
                    BuildActor(s.GetView(), s.Get<Skill>().GetAvailableSkillNames(), null)
                ).ToList() ?? new List<ActorSnapshot>()
            };
        }

        private static string NormalizeSkillName(string skillName)
        {
            var normalized = skillName.Trim().ToLowerInvariant();
            return string.IsNullOrWhiteSpace(normalized) ? "iron" : normalized;
        }

        private static string BuildTurnNote(bool playerOneTimedOut, bool playerTwoTimedOut)
        {
            if (playerOneTimedOut && playerTwoTimedOut)
            {
                return "Both players timed out and defaulted to iron 0.";
            }

            if (playerOneTimedOut)
            {
                return "Player one timed out and defaulted to iron 0.";
            }

            if (playerTwoTimedOut)
            {
                return "Player two timed out and defaulted to iron 0.";
            }

            return "Both players submitted in time.";
        }

        private static string ToReasonCode(MatchEndReason reason)
        {
            return reason switch
            {
                MatchEndReason.NormalDefeat => "normal-defeat",
                MatchEndReason.OpponentDisconnected => "opponent-disconnected",
                MatchEndReason.TimeoutForfeit => "timeout-forfeit",
                MatchEndReason.SimultaneousTimeoutDraw => "simultaneous-timeout-draw",
                _ => "unknown"
            };
        }

        private sealed record PendingTurn(string SkillName, int Param, string StringParam, bool TimedOut);

        private sealed record TurnLog(
            int Index,
            string PlayerOneSkill,
            int PlayerOneParam,
            string PlayerOneStringParam,
            string PlayerTwoSkill,
            int PlayerTwoParam,
            string PlayerTwoStringParam,
            bool PlayerOneTimedOut,
            bool PlayerTwoTimedOut,
            string Note);

        private sealed record MatchCompletion(string PlayerOneOutcome, string PlayerTwoOutcome, MatchEndReason Reason);

        private enum RoomParticipant
        {
            PlayerOne,
            PlayerTwo
        }

        private enum MatchEndReason
        {
            NormalDefeat,
            OpponentDisconnected,
            TimeoutForfeit,
            SimultaneousTimeoutDraw
        }
    }
}
