namespace YarpProxy.Endpoints;

public class SystemHealthEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/admin/system/status", async (
            [FromServices] IConfiguration configuration) =>
        {
            var services = new Dictionary<string, string>
            {
                { "Auth", configuration["Services:Auth"] ?? "http://127.0.0.1:8001/" },
                { "UserProfile", configuration["Services:UserProfile"] ?? "http://127.0.0.1:8002/" },
                { "Venue", configuration["Services:Venue"] ?? "http://127.0.0.1:8003/" },
                { "Billing", configuration["Services:Billing"] ?? "http://127.0.0.1:8004/" }
            };

            using var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(3);

            var tasks = services.Select(async service =>
            {
                var isOnline = false;
                try
                {
                    var response = await client.GetAsync($"{service.Value.TrimEnd('/')}/health");
                    isOnline = response.IsSuccessStatusCode;
                }
                catch
                {
                    isOnline = false;
                }

                return new { Name = service.Key, Status = isOnline ? "Online" : "Offline" };
            });

            var results = await Task.WhenAll(tasks);
            return Results.Ok(results.ToDictionary(r => r.Name, r => r.Status));
        })
        .WithTags("AdminSystem")
        .WithName("GetSystemStatus")
        .WithSummary("Получить реальный статус микросервисов")
        .RequireAuthorization(policy => policy.RequirePermissions(Permissions.AccountAdminRead));
    }
}
