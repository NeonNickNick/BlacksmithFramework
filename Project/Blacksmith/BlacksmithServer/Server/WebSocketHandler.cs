using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace BlacksmithServer.Server
{
    public class WebSocketHandler
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        private readonly RoomManager _roomManager;
        private readonly MatchmakingQueue _matchmakingQueue;

        public WebSocketHandler(RoomManager roomManager)
        {
            _roomManager = roomManager;
            _matchmakingQueue = new MatchmakingQueue(roomManager);
        }

        public int QueuedCount => _matchmakingQueue.Count;
        public int RoomCount => _roomManager.RoomCount;

        public async Task HandleAsync(HttpContext context)
        {
            if (!context.WebSockets.IsWebSocketRequest)
            {
                context.Response.StatusCode = 400;
                return;
            }

            var ws = await context.WebSockets.AcceptWebSocketAsync();
            var player = new Player(ws);
            Console.WriteLine($"[WS] Player {player.Id} connected");

            try
            {
                await ReceiveLoop(player);
            }
            catch (WebSocketException ex)
            {
                Console.WriteLine($"[WS] Player {player.Id} WebSocket error: {ex.Message}");
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine($"[WS] Player {player.Id} connection canceled");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WS] Player {player.Id} unexpected error: {ex}");
            }
            finally
            {
                Console.WriteLine($"[WS] Player {player.Id} disconnected, cleaning up");
                await HandleDisconnect(player);
            }
        }

        private async Task ReceiveLoop(Player player)
        {
            var buffer = new byte[4096];
            while (player.Socket.State == WebSocketState.Open)
            {
                WebSocketReceiveResult result;
                using var ms = new MemoryStream();
                do
                {
                    result = await player.Socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    ms.Write(buffer, 0, result.Count);
                } while (!result.EndOfMessage);

                if (result.MessageType == WebSocketMessageType.Close)
                    break;

                var json = Encoding.UTF8.GetString(ms.ToArray());
                await RouteMessage(player, json);
            }
        }

        private async Task RouteMessage(Player player, string json)
        {
            ClientMessage? msg;
            try
            {
                msg = JsonSerializer.Deserialize<ClientMessage>(json, JsonOptions);
            }
            catch
            {
                await player.SendAsync(new { type = MessageTypes.Error, message = "Invalid message format." });
                return;
            }

            if (msg == null) return;

            switch (msg.Type)
            {
                case MessageTypes.JoinQueue:
                    await _matchmakingQueue.EnqueueAsync(player);
                    break;

                case MessageTypes.LeaveQueue:
                    _matchmakingQueue.Dequeue(player);
                    await player.SendAsync(new { type = MessageTypes.Queued, position = -1 });
                    break;

                case MessageTypes.Declare:
                    var declare = JsonSerializer.Deserialize<DeclareMessage>(json, JsonOptions);
                    if (declare == null)
                    {
                        await player.SendAsync(new { type = MessageTypes.Error, message = "Invalid declare message." });
                        return;
                    }
                    var room = player.Room;
                    if (room == null)
                    {
                        await player.SendAsync(new { type = MessageTypes.Error, message = "You are not in a room." });
                        return;
                    }
                    await room.OnPlayerDeclare(player, declare.SkillName, declare.Param);
                    break;

                case MessageTypes.Ping:
                    await player.SendAsync(new { type = MessageTypes.Pong });
                    break;

                default:
                    await player.SendAsync(new { type = MessageTypes.Error, message = $"Unknown message type: {msg.Type}" });
                    break;
            }
        }

        private async Task HandleDisconnect(Player player)
        {
            _matchmakingQueue.Dequeue(player);

            if (player.Room != null)
            {
                try
                {
                    await player.Room.OnPlayerDisconnected(player);
                }
                catch
                {
                    // Best effort — other player may already be gone
                }
                _roomManager.RemoveRoom(player.Room.Id);
            }
        }
    }
}
