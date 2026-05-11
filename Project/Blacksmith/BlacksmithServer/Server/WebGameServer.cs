namespace BlacksmithServer.Server
{
    public static class WebGameServer
    {
        public static void Start()
        {
            var builder = WebApplication.CreateBuilder(new WebApplicationOptions
            {
                WebRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"),
                ContentRootPath = Directory.GetCurrentDirectory()
            });

            builder.WebHost.UseUrls("http://0.0.0.0:5000");
            var app = builder.Build();

            Console.WriteLine($"WebRootPath: {app.Environment.WebRootPath}");
            Console.WriteLine($"ContentRootPath: {app.Environment.ContentRootPath}");

            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseWebSockets();

            var roomManager = new RoomManager();
            var wsHandler = new WebSocketHandler(roomManager);

            app.Map("/ws", async (HttpContext context) =>
            {
                await wsHandler.HandleAsync(context);
            });

            app.MapGet("/api/health", () =>
            {
                return Results.Json(new
                {
                    rooms = wsHandler.RoomCount,
                    queued = wsHandler.QueuedCount
                });
            });

            Console.WriteLine("Blacksmith Multiplayer Server at http://+:5000/");

            app.Run();
        }
    }
}
