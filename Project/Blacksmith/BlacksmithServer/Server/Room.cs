using BlacksmithCore.Driver;
using BlacksmithCore.Infra.Models.Components;
using BlacksmithCore.Infra.Models.Entites;
using ClapInfra.ClapModels.Components;

namespace BlacksmithServer.Server
{
    public enum RoomState
    {
        Playing,
        Finished
    }

    public class Room
    {
        public Guid Id { get; } = Guid.NewGuid();
        public Player Player1 { get; }
        public Player Player2 { get; }
        public GameInstance Game { get; }
        public RoomState State { get; private set; } = RoomState.Playing;

        private (string skillName, int param)? _p1Pending;
        private (string skillName, int param)? _p2Pending;
        private CancellationTokenSource? _turnTimerCts;
        private readonly object _lock = new();

        private const int TurnSeconds = 15;
        private const int MaxConsecutiveTimeouts = 3;

        public Room(Player p1, Player p2)
        {
            Player1 = p1;
            Player2 = p2;
            p1.PlayerNumber = 1;
            p2.PlayerNumber = 2;
            p1.Room = this;
            p2.Room = this;

            var starter = new BackendStarter();
            Game = starter.StartBackend();

            _ = NotifyGameStart();
        }

        private async Task NotifyGameStart()
        {
            await Player1.SendAsync(new
            {
                type = MessageTypes.Matched,
                roomId = Id.ToString("N")[..8],
                playerNumber = 1
            });
            await Player2.SendAsync(new
            {
                type = MessageTypes.Matched,
                roomId = Id.ToString("N")[..8],
                playerNumber = 2
            });

            await Player1.SendAsync(new
            {
                type = MessageTypes.GameStart,
                snapshot = BuildSnapshotForPlayer(1)
            });
            await Player2.SendAsync(new
            {
                type = MessageTypes.GameStart,
                snapshot = BuildSnapshotForPlayer(2)
            });

            StartTurnTimer();
        }

        public Task OnPlayerDeclare(Player player, string skillName, int param)
        {
            lock (_lock)
            {
                if (State != RoomState.Playing)
                {
                    _ = player.SendAsync(new { type = MessageTypes.Error, message = "Game is not active." });
                    return Task.CompletedTask;
                }

                if (player.PlayerNumber == 1 && _p1Pending != null)
                    return Task.CompletedTask;
                if (player.PlayerNumber == 2 && _p2Pending != null)
                    return Task.CompletedTask;

                SkillDeclareResult result;
                if (player.PlayerNumber == 1)
                    result = Game.TryDeclare(skillName, param);
                else
                    result = Game.ETryDeclare(skillName, param);

                if (result != SkillDeclareResult.Success)
                {
                    _ = player.SendAsync(new { type = MessageTypes.Error, message = $"Skill '{skillName}' {result}." });
                    return Task.CompletedTask;
                }

                if (player.PlayerNumber == 1)
                    _p1Pending = (skillName, param);
                else
                    _p2Pending = (skillName, param);

                player.ConsecutiveTimeouts = 0;

                _ = player.SendAsync(new { type = MessageTypes.Waiting, message = "Waiting for opponent..." });

                if (_p1Pending != null && _p2Pending != null)
                {
                    CancelTurnTimer();
                    ResolveTurn();
                }
            }
            return Task.CompletedTask;
        }

        private void ResolveTurn()
        {
            var p1 = _p1Pending!.Value;
            var p2 = _p2Pending!.Value;
            _p1Pending = null;
            _p2Pending = null;

            Game.Declare(p1.skillName, p1.param, p2.skillName, p2.param);

            var pv = Game.Player.Focus.GetView();
            var ev = Game.Enemy.Focus.GetView();
            var result = DetermineResult(pv, ev);

            if (result != "next")
            {
                State = RoomState.Finished;
                _ = Player1.SendAsync(new
                {
                    type = MessageTypes.GameOver,
                    result = result == "win" ? "win" : result == "lose" ? "lose" : "draw",
                    snapshot = BuildSnapshotForPlayer(1)
                });
                _ = Player2.SendAsync(new
                {
                    type = MessageTypes.GameOver,
                    result = result == "win" ? "lose" : result == "lose" ? "win" : "draw",
                    snapshot = BuildSnapshotForPlayer(2)
                });
            }
            else
            {
                _ = Player1.SendAsync(new { type = MessageTypes.Snapshot, snapshot = BuildSnapshotForPlayer(1) });
                _ = Player2.SendAsync(new { type = MessageTypes.Snapshot, snapshot = BuildSnapshotForPlayer(2) });
                StartTurnTimer();
            }
        }

        private void StartTurnTimer()
        {
            CancelTurnTimer();
            _turnTimerCts = new CancellationTokenSource();
            var ct = _turnTimerCts.Token;

            _ = Player1.SendAsync(new { type = MessageTypes.TurnTimerStart, secondsRemaining = TurnSeconds });
            _ = Player2.SendAsync(new { type = MessageTypes.TurnTimerStart, secondsRemaining = TurnSeconds });

            _ = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(TurnSeconds), ct);
                    if (!ct.IsCancellationRequested)
                    {
                        HandleTurnTimeout();
                    }
                }
                catch (TaskCanceledException) { }
            });
        }

        private void CancelTurnTimer()
        {
            if (_turnTimerCts != null)
            {
                _turnTimerCts.Cancel();
                _turnTimerCts.Dispose();
                _turnTimerCts = null;
            }
        }

        private void HandleTurnTimeout()
        {
            lock (_lock)
            {
                if (State != RoomState.Playing)
                    return;

                if (_p1Pending == null)
                {
                    Player1.ConsecutiveTimeouts++;
                    if (Player1.ConsecutiveTimeouts >= MaxConsecutiveTimeouts)
                    {
                        ForfeitGame(Player1);
                        return;
                    }
                    _p1Pending = ("iron", 0);
                    _ = Player1.SendAsync(new { type = MessageTypes.Error, message = "Turn timeout. Auto-passing with 'iron'." });
                }

                if (_p2Pending == null)
                {
                    Player2.ConsecutiveTimeouts++;
                    if (Player2.ConsecutiveTimeouts >= MaxConsecutiveTimeouts)
                    {
                        ForfeitGame(Player2);
                        return;
                    }
                    _p2Pending = ("iron", 0);
                    _ = Player2.SendAsync(new { type = MessageTypes.Error, message = "Turn timeout. Auto-passing with 'iron'." });
                }

                if (_p1Pending != null && _p2Pending != null)
                {
                    CancelTurnTimer();
                    ResolveTurn();
                }
            }
        }

        private void ForfeitGame(Player timeoutPlayer)
        {
            State = RoomState.Finished;
            var winner = timeoutPlayer == Player1 ? Player2 : Player1;
            var loser = timeoutPlayer;

            _ = winner.SendAsync(new
            {
                type = MessageTypes.GameOver,
                result = "win",
                message = "Opponent forfeited due to repeated timeouts.",
                snapshot = BuildSnapshotForPlayer(winner.PlayerNumber)
            });
            _ = loser.SendAsync(new
            {
                type = MessageTypes.GameOver,
                result = "lose",
                message = "You forfeited due to repeated timeouts.",
                snapshot = BuildSnapshotForPlayer(loser.PlayerNumber)
            });
        }

        public async Task OnPlayerDisconnected(Player player)
        {
            lock (_lock)
            {
                if (State != RoomState.Playing) return;
                State = RoomState.Finished;
            }

            CancelTurnTimer();
            var other = player == Player1 ? Player2 : Player1;
            await other.SendAsync(new
            {
                type = MessageTypes.OpponentDisconnected,
                message = "Opponent disconnected. You win!"
            });
        }

        public void Cleanup()
        {
            CancelTurnTimer();
        }

        private object BuildSnapshotForPlayer(int playerNumber)
        {
            var playerBody = playerNumber == 1 ? Game.Player : Game.Enemy;
            var enemyBody = playerNumber == 1 ? Game.Enemy : Game.Player;

            var pv = playerBody.Focus.GetView();
            var ev = enemyBody.Focus.GetView();

            var result = DetermineResult(pv, ev);

            return new
            {
                player = BuildActor(pv, playerBody.Focus.Get<Skill>().GetAvailableSkillNames()),
                enemy = BuildActor(ev, enemyBody.Focus.Get<Skill>().GetAvailableSkillNames()),
                turns = Game.History.SkillHistory.Select((pair, i) =>
                {
                    var pSkill = playerNumber == 1 ? pair.Item1 : pair.Item2;
                    var eSkill = playerNumber == 1 ? pair.Item2 : pair.Item1;
                    return new
                    {
                        index = i + 1,
                        result = "Continue",
                        playerSkill = pSkill.SkillName,
                        playerParam = pSkill.Param,
                        enemySkill = eSkill.SkillName,
                        enemyParam = eSkill.Param
                    };
                }).ToList(),
                started = true,
                manualMode = false,
                modeName = "PvP",
                result
            };
        }

        private static object BuildActor(BodyView view, List<string> availableSkills)
        {
            return new
            {
                professions = view.ProfessionNames,
                hp = view.HP,
                maxHP = view.MHP,
                defenses = view.DefenseView.Select(d => new { name = d.name, power = d.power }).ToList(),
                resources = view.ResourcesView.Select(r => new { name = r.name, quantity = r.quantity }).ToList(),
                futureAttacks = view.FutureAttackView.Select(f => new
                {
                    name = f.name,
                    delayRounds = f.delayRounds,
                    power = f.power
                }).ToList(),
                futureDefenses = view.FutureDefenseView.Select(f => new
                {
                    name = f.name,
                    delayRounds = f.delayRounds,
                    power = f.power
                }).ToList(),
                availableSkills
            };
        }

        private static string DetermineResult(BodyView player, BodyView enemy)
        {
            bool playerDead = player.HP <= 0;
            bool enemyDead = enemy.HP <= 0;
            if (playerDead && enemyDead) return "draw";
            if (playerDead) return "lose";
            if (enemyDead) return "win";
            return "next";
        }
    }
}
