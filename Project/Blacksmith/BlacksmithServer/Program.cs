using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using BlacksmithCore.Infra.Utils;
using BlacksmithServer.Web;
using BlacksmithServer.Web.Auth;
using BlacksmithServer.Web.Realtime;
namespace BlacksmithServer
{
    public static class Server
    {
        public static void Main()
        {
            ModLoader.Initialize(AppContext.BaseDirectory);

            var builder = WebApplication.CreateBuilder(new WebApplicationOptions
            {
                WebRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"),
                ContentRootPath = Directory.GetCurrentDirectory()
            });

            builder.WebHost.UseUrls("http://0.0.0.0:5000");

            builder.Services.AddSingleton<UserStore>();
            builder.Services.AddSingleton<AuthService>();
            builder.Services.AddSingleton<ConnectionRegistry>();
            builder.Services.AddSingleton<MatchCoordinator>();

            var app = builder.Build();

            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseWebSockets();

            app.MapGet("/api/health", () => Results.Json(new { ok = true, message = "BlacksmithServer online." }));

            app.MapPost("/api/auth/register", async (AuthRequest request, AuthService authService) =>
            {
                return Results.Json(await authService.RegisterAsync(request.Username, request.Password));
            });

            app.MapPost("/api/auth/login", async (AuthRequest request, AuthService authService) =>
            {
                return Results.Json(await authService.LoginAsync(request.Username, request.Password));
            });

            app.MapGet("/api/auth/status", (HttpContext context, AuthService authService) =>
            {
                var token = GetBearerToken(context);
                if (!authService.TryGetUsername(token, out var username))
                {
                    return Results.Json(new AuthResponse
                    {
                        Ok = false,
                        Message = "Unauthorized."
                    });
                }

                return Results.Json(new AuthResponse
                {
                    Ok = true,
                    Message = "Authenticated.",
                    Token = token,
                    Username = username
                });
            });

            app.MapPost("/api/auth/logout", (HttpContext context, AuthService authService) =>
            {
                var token = GetBearerToken(context);
                if (!string.IsNullOrWhiteSpace(token))
                {
                    authService.Logout(token);
                }

                return Results.Json(new { ok = true });
            });

            app.Map("/ws", async (HttpContext context, AuthService authService, ConnectionRegistry connections, MatchCoordinator coordinator) =>
            {
                if (!context.WebSockets.IsWebSocketRequest)
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    await context.Response.WriteAsync("WebSocket request expected.");
                    return;
                }

                var token = context.Request.Query["token"].ToString();
                if (!authService.TryGetUsername(token, out var username))
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("Unauthorized.");
                    return;
                }

                using var socket = await context.WebSockets.AcceptWebSocketAsync();
                await connections.RegisterAsync(username, socket);
                await coordinator.SendCurrentStateAsync(username, "Connected to BlacksmithServer.");

                try
                {
                    while (socket.State == WebSocketState.Open && !context.RequestAborted.IsCancellationRequested)
                    {
                        var payload = await ReceiveTextAsync(socket, context.RequestAborted);
                        if (payload == null)
                        {
                            break;
                        }

                        await HandleClientMessageAsync(payload, username, coordinator);
                    }
                }
                catch (OperationCanceledException)
                {
                }
                finally
                {
                    if (connections.RemoveIfCurrent(username, socket))
                    {
                        await coordinator.HandleDisconnectedAsync(username);
                    }

                    if (socket.State is WebSocketState.Open or WebSocketState.CloseReceived)
                    {
                        try
                        {
                            await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing.", CancellationToken.None);
                        }
                        catch
                        {
                        }
                    }
                }
            });

            app.Run();
        }

        static string? GetBearerToken(HttpContext context)
        {
            var header = context.Request.Headers.Authorization.ToString();
            if (header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                return header["Bearer ".Length..].Trim();
            }

            return null;
        }

        static async Task<string?> ReceiveTextAsync(WebSocket socket, CancellationToken cancellationToken)
        {
            var buffer = new byte[4096];
            using var ms = new MemoryStream();

            while (true)
            {
                var result = await socket.ReceiveAsync(buffer, cancellationToken);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    return null;
                }

                ms.Write(buffer, 0, result.Count);
                if (result.EndOfMessage)
                {
                    break;
                }
            }

            return Encoding.UTF8.GetString(ms.ToArray());
        }

        static async Task HandleClientMessageAsync(string payload, string username, MatchCoordinator coordinator)
        {
            try
            {
                using var document = JsonDocument.Parse(payload);
                var root = document.RootElement;
                if (!root.TryGetProperty("type", out var typeElement))
                {
                    await coordinator.SendCurrentStateAsync(username, "Message type is required.");
                    return;
                }

                var type = typeElement.GetString()?.Trim();
                switch (type)
                {
                    case "queue":
                        await coordinator.EnqueueAsync(username);
                        break;
                    case "cancelQueue":
                        await coordinator.CancelQueueAsync(username, "Matchmaking cancelled.");
                        break;
                    case "submitTurn":
                        var skillName = ReadString(root, "skillName") ?? "iron";
                        var param = ReadInt(root, "param");
                        var stringParam = ReadString(root, "stringParam") ?? "";
                        Console.WriteLine($"[Turn] {username}: skill='{skillName}' param={param} stringParam='{stringParam}' raw='{root.GetRawText()}'");
                        await coordinator.SubmitTurnAsync(username, skillName, param, stringParam);
                        break;
                    case "requestSnapshot":
                        await coordinator.SendCurrentStateAsync(username);
                        break;
                    default:
                        await coordinator.SendCurrentStateAsync(username, $"Unsupported message type '{type}'.");
                        break;
                }
            }
            catch (JsonException)
            {
                await coordinator.SendCurrentStateAsync(username, "Invalid message payload.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error] HandleClientMessage for {username}: {ex}");
                await coordinator.SendCurrentStateAsync(username, "An internal error occurred.");
            }
        }

        static string? ReadString(JsonElement root, string propertyName)
        {
            if (root.TryGetProperty(propertyName, out var element) && element.ValueKind == JsonValueKind.String)
            {
                return element.GetString();
            }

            return null;
        }

        static int ReadInt(JsonElement root, string propertyName)
        {
            if (!root.TryGetProperty(propertyName, out var element))
            {
                return 0;
            }

            if (element.ValueKind == JsonValueKind.Number && element.TryGetInt32(out var number))
            {
                return number;
            }

            if (element.ValueKind == JsonValueKind.String && int.TryParse(element.GetString(), out number))
            {
                return number;
            }

            return 0;
        }
    }
}