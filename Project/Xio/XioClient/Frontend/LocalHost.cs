namespace XioClient.Frontend
{
    public static class LocalHost
    {
        public static void Start()
        {
            var builder = WebApplication.CreateBuilder(new WebApplicationOptions
            {
                WebRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"),
                ContentRootPath = Directory.GetCurrentDirectory()
            });

            builder.WebHost.UseUrls("http://localhost:5001");
            var app = builder.Build();

            Console.WriteLine($"WebRootPath: {app.Environment.WebRootPath}");
            Console.WriteLine($"ContentRootPath: {app.Environment.ContentRootPath}");

            app.UseDefaultFiles();
            app.UseStaticFiles();

            WebGameSession webGameSession = new();

            app.MapGet("/api/strategies", () =>
            {
                return Results.Json(webGameSession.GetStrategies());
            });

            app.MapPost("/api/start", async (HttpContext ctx) =>
            {
                var dto = await ctx.Request.ReadFromJsonAsync<StartDto>();
                var snapshot = webGameSession.StartGame(dto?.mode ?? 0);
                return Results.Json(new { ok = true, snapshot });
            });

            app.MapPost("/api/declare", async (HttpContext ctx) =>
            {
                var dto = await ctx.Request.ReadFromJsonAsync<DeclareDto>();
                if (dto == null)
                {
                    return Results.Json(new { ok = false, message = "Invalid input", snapshot = webGameSession.GetSnapshot() });
                }

                var result = webGameSession.DeclareTurn(dto.skillInput ?? string.Empty, dto.enemySkillInput ?? string.Empty);
                return Results.Json(new { ok = result.Ok, message = result.Message, snapshot = result.Snapshot });
            });

            app.MapGet("/api/status", () =>
            {
                return Results.Json(new { ok = true, snapshot = webGameSession.GetSnapshot() });
            });

            Console.WriteLine("Starting Xio local web host at http://localhost:5001/");

            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = "http://localhost:5001",
                UseShellExecute = true
            });

            app.Run();
        }

        private class StartDto
        {
            public int mode { get; set; }
        }

        private class DeclareDto
        {
            public string? skillInput { get; set; }
            public string? enemySkillInput { get; set; }
        }
    }
}
