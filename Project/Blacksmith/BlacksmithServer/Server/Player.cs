using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace BlacksmithServer.Server
{
    public class Player
    {
        public string Id { get; } = Guid.NewGuid().ToString("N")[..8];
        public WebSocket Socket { get; }
        public Room? Room { get; set; }
        public int PlayerNumber { get; set; }
        public int ConsecutiveTimeouts { get; set; }

        public Player(WebSocket socket)
        {
            Socket = socket;
        }

        public async Task SendAsync(object message)
        {
            var json = JsonSerializer.Serialize(message);
            var bytes = Encoding.UTF8.GetBytes(json);
            await Socket.SendAsync(bytes, WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }
}
