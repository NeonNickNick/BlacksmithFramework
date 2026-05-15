namespace BlacksmithServer.Web.Realtime
{
    public sealed class MatchCoordinator
    {
        private const int QueueTimeoutSeconds = 30;
        private static readonly TimeSpan QueueTimeout = TimeSpan.FromSeconds(QueueTimeoutSeconds);

        private readonly ConnectionRegistry _connections;
        private readonly SemaphoreSlim _gate = new(1, 1);
        private readonly List<QueueEntry> _queue = new();
        private readonly Dictionary<string, QueueEntry> _queueByUser = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, GameRoom> _roomsByUser = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, BattleSnapshot> _lastSnapshots = new(StringComparer.OrdinalIgnoreCase);

        public MatchCoordinator(ConnectionRegistry connections)
        {
            _connections = connections;
        }

        public async Task SendCurrentStateAsync(string username, string? message = null)
        {
            BattleSnapshot snapshot;

            await _gate.WaitAsync();
            try
            {
                if (_roomsByUser.TryGetValue(username, out var room))
                {
                    snapshot = room.BuildSnapshot(username);
                }
                else if (_queueByUser.TryGetValue(username, out var entry))
                {
                    snapshot = BuildQueueSnapshot(username, entry.ExpiresAtUtc, $"Searching for an opponent. Matchmaking will stop after {QueueTimeoutSeconds} seconds.");
                }
                else if (_lastSnapshots.TryGetValue(username, out var lastSnapshot) && lastSnapshot.Status == "finished")
                {
                    snapshot = lastSnapshot;
                }
                else
                {
                    snapshot = BuildIdleSnapshot(username, "Logged in. Click Find Match to enter the queue.");
                }
            }
            finally
            {
                _gate.Release();
            }

            await RecordAndSendSnapshotAsync(username, snapshot, message);
        }

        public async Task EnqueueAsync(string username)
        {
            QueueEntry? queueEntry = null;
            QueueEntry? opponent = null;
            GameRoom? room = null;
            var createdQueueEntry = false;
            var createdRoom = false;

            await _gate.WaitAsync();
            try
            {
                if (_roomsByUser.ContainsKey(username))
                {
                    room = _roomsByUser[username];
                }
                else if (_queueByUser.TryGetValue(username, out queueEntry))
                {
                }
                else
                {
                    opponent = _queue.FirstOrDefault(q => !string.Equals(q.Username, username, StringComparison.OrdinalIgnoreCase));
                    if (opponent == null)
                    {
                        queueEntry = new QueueEntry(username, DateTimeOffset.UtcNow.Add(QueueTimeout));
                        _queue.Add(queueEntry);
                        _queueByUser[username] = queueEntry;
                        createdQueueEntry = true;
                    }
                    else
                    {
                        RemoveQueueEntryNoLock(opponent);
                        room = new GameRoom(
                            opponent.Username,
                            username,
                            RecordAndSendSnapshotAsync,
                            OnRoomCompletedAsync);
                        _roomsByUser[opponent.Username] = room;
                        _roomsByUser[username] = room;
                        createdRoom = true;
                    }
                }
            }
            finally
            {
                _gate.Release();
            }

            if (createdRoom && room != null)
            {
                await room.StartAsync();
                return;
            }

            if (createdQueueEntry && queueEntry != null)
            {
                StartQueueTimeout(queueEntry);
                await RecordAndSendSnapshotAsync(
                    username,
                    BuildQueueSnapshot(username, queueEntry.ExpiresAtUtc, $"Searching for an opponent. Matchmaking will stop after {QueueTimeoutSeconds} seconds."),
                    "Queue started.");
                return;
            }

            await SendCurrentStateAsync(username, "You are already queued or in a match.");
        }

        public async Task CancelQueueAsync(string username, string reason)
        {
            var removed = false;

            await _gate.WaitAsync();
            try
            {
                if (_queueByUser.TryGetValue(username, out var entry))
                {
                    RemoveQueueEntryNoLock(entry);
                    removed = true;
                }
            }
            finally
            {
                _gate.Release();
            }

            if (removed)
            {
                await RecordAndSendSnapshotAsync(username, BuildIdleSnapshot(username, "Logged in. Click Find Match to enter the queue."), reason);
            }
            else
            {
                await SendCurrentStateAsync(username, "You are not currently queued.");
            }
        }

        public async Task SubmitTurnAsync(string username, string skillName, int param, string stringParam = "")
        {
            GameRoom? room;

            await _gate.WaitAsync();
            try
            {
                _roomsByUser.TryGetValue(username, out room);
            }
            finally
            {
                _gate.Release();
            }

            if (room == null)
            {
                await SendCurrentStateAsync(username, "You are not currently in a match.");
                return;
            }

            await room.SubmitTurnAsync(username, skillName, param, stringParam);
        }

        public async Task HandleDisconnectedAsync(string username)
        {
            GameRoom? room = null;

            await _gate.WaitAsync();
            try
            {
                if (_queueByUser.TryGetValue(username, out var entry))
                {
                    RemoveQueueEntryNoLock(entry);
                }

                _roomsByUser.TryGetValue(username, out room);
            }
            finally
            {
                _gate.Release();
            }

            if (room != null)
            {
                await room.HandleDisconnectAsync(username);
            }
        }

        private async Task OnRoomCompletedAsync(GameRoom room)
        {
            await _gate.WaitAsync();
            try
            {
                if (_roomsByUser.TryGetValue(room.PlayerOneUsername, out var firstRoom) && ReferenceEquals(firstRoom, room))
                {
                    _roomsByUser.Remove(room.PlayerOneUsername);
                }

                if (_roomsByUser.TryGetValue(room.PlayerTwoUsername, out var secondRoom) && ReferenceEquals(secondRoom, room))
                {
                    _roomsByUser.Remove(room.PlayerTwoUsername);
                }
            }
            finally
            {
                _gate.Release();
            }
        }

        private void StartQueueTimeout(QueueEntry entry)
        {
            if (entry.TimeoutTask != null)
            {
                return;
            }

            entry.TimeoutTask = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(QueueTimeout, entry.TimeoutCts.Token);
                }
                catch (OperationCanceledException)
                {
                    return;
                }

                var removed = false;

                await _gate.WaitAsync();
                try
                {
                    if (_queueByUser.TryGetValue(entry.Username, out var current) && ReferenceEquals(current, entry))
                    {
                        RemoveQueueEntryNoLock(entry);
                        removed = true;
                    }
                }
                finally
                {
                    _gate.Release();
                }

                if (removed)
                {
                    await RecordAndSendSnapshotAsync(
                        entry.Username,
                        BuildIdleSnapshot(entry.Username, $"No opponent was found within {QueueTimeoutSeconds} seconds. Click Find Match to try again."),
                        $"No opponent found within {QueueTimeoutSeconds} seconds.");
                }
            });
        }

        private void RemoveQueueEntryNoLock(QueueEntry entry)
        {
            entry.TimeoutCts.Cancel();
            entry.TimeoutCts.Dispose();
            _queue.Remove(entry);
            _queueByUser.Remove(entry.Username);
        }

        private async Task RecordAndSendSnapshotAsync(string username, BattleSnapshot snapshot, string? message)
        {
            await _gate.WaitAsync();
            try
            {
                _lastSnapshots[username] = snapshot;
            }
            finally
            {
                _gate.Release();
            }

            await _connections.SendAsync(username, new ServerEnvelope
            {
                Type = "snapshot",
                Message = message,
                Snapshot = snapshot
            });
        }

        private static BattleSnapshot BuildIdleSnapshot(string username, string statusMessage)
        {
            return new BattleSnapshot
            {
                Authenticated = true,
                Username = username,
                Status = "lobby",
                Started = false,
                ManualMode = false,
                ModeName = "Online Match",
                Result = "idle",
                Queued = false,
                HasSubmittedTurn = false,
                PlayerTimeouts = 0,
                EnemyTimeouts = 0,
                StatusMessage = statusMessage,
                Turns = new()
            };
        }

        private static BattleSnapshot BuildQueueSnapshot(string username, DateTimeOffset expiresAtUtc, string statusMessage)
        {
            return new BattleSnapshot
            {
                Authenticated = true,
                Username = username,
                Status = "queueing",
                Started = false,
                ManualMode = false,
                ModeName = "Online Match",
                Result = "idle",
                Queued = true,
                QueueExpiresAtUtc = expiresAtUtc.ToString("O"),
                HasSubmittedTurn = false,
                PlayerTimeouts = 0,
                EnemyTimeouts = 0,
                StatusMessage = statusMessage,
                Turns = new()
            };
        }

        private sealed class QueueEntry
        {
            public string Username { get; }
            public DateTimeOffset ExpiresAtUtc { get; }
            public CancellationTokenSource TimeoutCts { get; } = new();
            public Task? TimeoutTask { get; set; }

            public QueueEntry(string username, DateTimeOffset expiresAtUtc)
            {
                Username = username;
                ExpiresAtUtc = expiresAtUtc;
            }
        }
    }
}
