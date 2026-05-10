using System.Collections.Concurrent;

namespace BlacksmithServer.Server
{
    public class RoomManager
    {
        private readonly ConcurrentDictionary<Guid, Room> _rooms = new();
        private readonly CancellationTokenSource _cleanupCts = new();

        public int RoomCount => _rooms.Count;

        public RoomManager()
        {
            _ = RunCleanupLoop(_cleanupCts.Token);
        }

        public Room CreateRoom(Player p1, Player p2)
        {
            var room = new Room(p1, p2);
            _rooms.TryAdd(room.Id, room);
            return room;
        }

        public Room? GetRoom(Guid id)
        {
            _rooms.TryGetValue(id, out var room);
            return room;
        }

        public void RemoveRoom(Guid id)
        {
            if (_rooms.TryRemove(id, out var room))
            {
                room.Cleanup();
            }
        }

        private async Task RunCleanupLoop(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(30), ct);
                }
                catch (TaskCanceledException) { break; }

                foreach (var (id, room) in _rooms)
                {
                    if (room.State == RoomState.Finished)
                    {
                        _rooms.TryRemove(id, out _);
                        room.Cleanup();
                    }
                }
            }
        }

        public void Stop()
        {
            _cleanupCts.Cancel();
            _cleanupCts.Dispose();
            foreach (var (_, room) in _rooms)
            {
                room.Cleanup();
            }
            _rooms.Clear();
        }
    }
}
