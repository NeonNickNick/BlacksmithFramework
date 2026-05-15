using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace BlacksmithServer.Web.Realtime
{
    public sealed class ConnectionRegistry
    {
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

        private readonly ConcurrentDictionary<string, SocketConnection> _connections = new(StringComparer.OrdinalIgnoreCase);

        public async Task RegisterAsync(string username, WebSocket socket)
        {
            var replacement = new SocketConnection(socket);
            SocketConnection? previous = null;

            _connections.AddOrUpdate(
                username,
                _ => replacement,
                (_, existing) =>
                {
                    previous = existing;
                    return replacement;
                });

            if (previous != null)
            {
                await previous.CloseAsync("Replaced by a newer connection.");
            }
        }

        public bool RemoveIfCurrent(string username, WebSocket socket)
        {
            if (!_connections.TryGetValue(username, out var current) || !ReferenceEquals(current.Socket, socket))
            {
                return false;
            }

            if (_connections.TryRemove(username, out var removed))
            {
                removed.Dispose();
                return true;
            }

            return false;
        }

        public async Task SendAsync(string username, ServerEnvelope envelope, CancellationToken cancellationToken = default)
        {
            if (!_connections.TryGetValue(username, out var connection))
            {
                return;
            }

            var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(envelope, JsonOptions));
            await connection.SendAsync(bytes, cancellationToken);
        }

        private sealed class SocketConnection : IDisposable
        {
            private readonly SemaphoreSlim _sendGate = new(1, 1);

            public WebSocket Socket { get; }

            public SocketConnection(WebSocket socket)
            {
                Socket = socket;
            }

            public async Task SendAsync(byte[] payload, CancellationToken cancellationToken)
            {
                if (Socket.State != WebSocketState.Open)
                {
                    return;
                }

                await _sendGate.WaitAsync(cancellationToken);
                try
                {
                    if (Socket.State == WebSocketState.Open)
                    {
                        await Socket.SendAsync(payload, WebSocketMessageType.Text, true, cancellationToken);
                    }
                }
                catch
                {
                }
                finally
                {
                    _sendGate.Release();
                }
            }

            public async Task CloseAsync(string reason)
            {
                try
                {
                    if (Socket.State is WebSocketState.Open or WebSocketState.CloseReceived)
                    {
                        await Socket.CloseAsync(WebSocketCloseStatus.NormalClosure, reason, CancellationToken.None);
                    }
                }
                catch
                {
                }
            }

            public void Dispose()
            {
                _sendGate.Dispose();
            }
        }
    }
}
