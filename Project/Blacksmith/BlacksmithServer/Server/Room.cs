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
                var r1 = result == "win" ? "win" : result == "lose" ? "lose" : "draw";
                var r2 = result == "win" ? "lose" : result == "lose" ? "win" : "draw";

                object? snap1 = null, snap2 = null;
                try { snap1 = BuildSnapshotForPlayer(1); } catch (Exception ex) { Console.WriteLine($"[Room] BuildSnapshot failed: {ex}"); }
                try { snap2 = BuildSnapshotForPlayer(2); } catch (Exception ex) { Console.WriteLine($"[Room] BuildSnapshot failed: {ex}"); }

                try { _ = Player1.SendAsync(new { type = MessageTypes.GameOver, result = r1, snapshot = snap1! }); }
                catch (Exception ex) { Console.WriteLine($"[Room] Send game_over P1 failed: {ex.Message}"); TrySendMinimalGameOver(Player1, r1); }

                try { _ = Player2.SendAsync(new { type = MessageTypes.GameOver, result = r2, snapshot = snap2! }); }
                catch (Exception ex) { Console.WriteLine($"[Room] Send game_over P2 failed: {ex.Message}"); TrySendMinimalGameOver(Player2, r2); }
            }
            else
            {
                object? snap1 = null, snap2 = null;
                try { snap1 = BuildSnapshotForPlayer(1); } catch (Exception ex) { Console.WriteLine($"[Room] BuildSnapshot failed: {ex}"); }
                try { snap2 = BuildSnapshotForPlayer(2); } catch (Exception ex) { Console.WriteLine($"[Room] BuildSnapshot failed: {ex}"); }

                try { _ = Player1.SendAsync(new { type = MessageTypes.Snapshot, snapshot = snap1! }); }
                catch (Exception ex) { Console.WriteLine($"[Room] Send snapshot P1 failed: {ex.Message}"); }

                try { _ = Player2.SendAsync(new { type = MessageTypes.Snapshot, snapshot = snap2! }); }
                catch (Exception ex) { Console.WriteLine($"[Room] Send snapshot P2 failed: {ex.Message}"); }

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
            Action? sendForfeitMessages = null;

            lock (_lock)
            {
                if (State != RoomState.Playing)
                    return;

                if (_p1Pending == null)
                {
                    Player1.ConsecutiveTimeouts++;
                    _p1Pending = ("iron", 0);
                    _ = Player1.SendAsync(new { type = MessageTypes.Error, message = "Turn timeout. Auto-passing with 'iron'." });
                }

                if (_p2Pending == null)
                {
                    Player2.ConsecutiveTimeouts++;
                    _p2Pending = ("iron", 0);
                    _ = Player2.SendAsync(new { type = MessageTypes.Error, message = "Turn timeout. Auto-passing with 'iron'." });
                }

                bool p1Forfeit = Player1.ConsecutiveTimeouts >= MaxConsecutiveTimeouts;
                bool p2Forfeit = Player2.ConsecutiveTimeouts >= MaxConsecutiveTimeouts;

                if (p1Forfeit && p2Forfeit)
                {
                    CancelTurnTimer();
                    ResolveTurn();
                    object? snap1 = null, snap2 = null;
                    try { snap1 = BuildSnapshotForPlayer(1); } catch (Exception ex) { Console.WriteLine($"[Room] BuildSnapshot P1 failed: {ex}"); }
                    try { snap2 = BuildSnapshotForPlayer(2); } catch (Exception ex) { Console.WriteLine($"[Room] BuildSnapshot P2 failed: {ex}"); }
                    State = RoomState.Finished;
                    sendForfeitMessages = () => SendForfeitDrawMessages(snap1, snap2, Player1, Player2);
                    return;
                }
                if (p1Forfeit)
                {
                    CancelTurnTimer();
                    ResolveTurn();
                    object? wSnap = null, lSnap = null;
                    try { wSnap = BuildSnapshotForPlayer(Player2.PlayerNumber); } catch (Exception ex) { Console.WriteLine($"[Room] BuildSnapshot winner failed: {ex}"); }
                    try { lSnap = BuildSnapshotForPlayer(Player1.PlayerNumber); } catch (Exception ex) { Console.WriteLine($"[Room] BuildSnapshot loser failed: {ex}"); }
                    State = RoomState.Finished;
                    sendForfeitMessages = () => SendForfeitMessages(Player2, Player1, wSnap, lSnap);
                    return;
                }
                if (p2Forfeit)
                {
                    CancelTurnTimer();
                    ResolveTurn();
                    object? wSnap = null, lSnap = null;
                    try { wSnap = BuildSnapshotForPlayer(Player1.PlayerNumber); } catch (Exception ex) { Console.WriteLine($"[Room] BuildSnapshot winner failed: {ex}"); }
                    try { lSnap = BuildSnapshotForPlayer(Player2.PlayerNumber); } catch (Exception ex) { Console.WriteLine($"[Room] BuildSnapshot loser failed: {ex}"); }
                    State = RoomState.Finished;
                    sendForfeitMessages = () => SendForfeitMessages(Player1, Player2, wSnap, lSnap);
                    return;
                }

                CancelTurnTimer();
                ResolveTurn();
            }

            sendForfeitMessages?.Invoke();
        }

        private static void SendForfeitMessages(Player winner, Player loser, object? winnerSnapshot, object? loserSnapshot)
        {
            try
            {
                _ = winner.SendAsync(new
                {
                    type = MessageTypes.GameOver,
                    result = "win",
                    message = "Opponent forfeited due to repeated timeouts.",
                    snapshot = winnerSnapshot!
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Room] Failed to send forfeit win message to {winner.Id}: {ex.Message}");
                TrySendMinimalGameOver(winner, "win");
            }

            try
            {
                _ = loser.SendAsync(new
                {
                    type = MessageTypes.GameOver,
                    result = "lose",
                    message = "You forfeited due to repeated timeouts.",
                    snapshot = loserSnapshot!
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Room] Failed to send forfeit lose message to {loser.Id}: {ex.Message}");
                TrySendMinimalGameOver(loser, "lose");
            }
        }

        private static void SendForfeitDrawMessages(object? p1Snapshot, object? p2Snapshot, Player player1, Player player2)
        {
            try
            {
                _ = player1.SendAsync(new
                {
                    type = MessageTypes.GameOver,
                    result = "draw",
                    message = "Both players forfeited due to repeated timeouts.",
                    snapshot = p1Snapshot!
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Room] Failed to send forfeit draw message to P1 {player1.Id}: {ex.Message}");
                TrySendMinimalGameOver(player1, "draw");
            }

            try
            {
                _ = player2.SendAsync(new
                {
                    type = MessageTypes.GameOver,
                    result = "draw",
                    message = "Both players forfeited due to repeated timeouts.",
                    snapshot = p2Snapshot!
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Room] Failed to send forfeit draw message to P2 {player2.Id}: {ex.Message}");
                TrySendMinimalGameOver(player2, "draw");
            }
        }

        private static void TrySendMinimalGameOver(Player player, string result)
        {
            try
            {
                _ = player.SendAsync(new { type = MessageTypes.GameOver, result });
            }
            catch
            {
                Console.WriteLine($"[Room] Could not send any game_over to {player.Id}");
            }
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

        private string DetermineResult(BodyView player, BodyView enemy)
        {
            bool playerDead = player.HP <= 0 || Player1.ConsecutiveTimeouts >= MaxConsecutiveTimeouts;
            bool enemyDead = enemy.HP <= 0 || Player2.ConsecutiveTimeouts >= MaxConsecutiveTimeouts;
            if (playerDead && enemyDead) return "draw";
            if (playerDead) return "lose";
            if (enemyDead) return "win";
            return "next";
        }
    }
}
