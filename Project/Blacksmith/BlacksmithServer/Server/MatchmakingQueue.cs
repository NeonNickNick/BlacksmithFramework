namespace BlacksmithServer.Server
{
    public class MatchmakingQueue
    {
        private readonly List<Player> _queue = new();
        private readonly object _lock = new();
        private readonly RoomManager _roomManager;

        public int Count { get { lock (_lock) return _queue.Count; } }

        public MatchmakingQueue(RoomManager roomManager)
        {
            _roomManager = roomManager;
        }

        public async Task EnqueueAsync(Player player)
        {
            Player? opponent = null;

            lock (_lock)
            {
                _queue.Add(player);
                if (_queue.Count >= 2)
                {
                    var p1 = _queue[0];
                    var p2 = _queue[1];
                    _queue.RemoveRange(0, 2);
                    opponent = p1 == player ? p2 : p1;
                }
            }

            if (opponent != null)
            {
                _roomManager.CreateRoom(player, opponent);
            }
            else
            {
                await player.SendAsync(new { type = MessageTypes.Queued, position = _queue.Count });
            }
        }

        public void Dequeue(Player player)
        {
            lock (_lock)
            {
                _queue.Remove(player);
            }
        }
    }
}
