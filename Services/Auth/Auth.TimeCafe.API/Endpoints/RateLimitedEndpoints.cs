namespace Auth.TimeCafe.API.Endpoints;

public class RateLimitedEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/test-rate-limit", (
            [FromServices] HttpContext context, 
            [FromServices] RateLimitConfig cfg) =>
        {

            return Results.Ok(new
            {
                success = true,
                time = DateTimeOffset.UtcNow
            });
        })
        .RequireRateLimiting("OneRequestPerInterval")
        .RequireRateLimiting("MaxRequestPerWindow")
        .RequireAuthorization()
        .WithTags("RateLimit")
        .WithName("TestRateLimit")
        .WithSummary("Тестовый эндпоинт для проверки rate limiting")
        .WithDescription("Возвращает успешный ответ с текущим временем. Применяются лимиты OneRequestPerInterval и MaxRequestPerWindow");

        app.MapGet("/api/test-rate-limit2", (
            [FromServices] HttpContext context, 
            [FromServices] RateLimitConfig cfg) =>
        {
            context.Response.Headers["X-Rate-Limit-Window"] = cfg.MinIntervalSeconds.ToString();

            return Results.Ok(new
            {
                success = true,
                time = DateTimeOffset.UtcNow
            });
        })
        .RequireRateLimiting("OneRequestPerInterval")
        .RequireRateLimiting("MaxRequestPerWindow")
        .RequireAuthorization()
        .WithTags("RateLimit")
        .WithName("TestRateLimit2")
        .WithSummary("Тестовый эндпоинт для проверки rate limiting с заголовком окна")
        .WithDescription("Возвращает успешный ответ и добавляет заголовок X-Rate-Limit-Window с интервалом");


        app.MapGet("/protected-test",
        async (
        [FromServices] UserManager<ApplicationUser> userManager,
        ClaimsPrincipal user) =>
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Results.Unauthorized();
            var u = await userManager.FindByIdAsync(userId);
            return Results.Ok($"Protected OK. User: {u?.Email} ({userId})");
        })
        .RequireAuthorization()
        .WithTags("Authentication")
        .WithName("Test401")
        .WithSummary("Тестовый эндпоинт для проверки авторизации")
        .WithDescription("Возвращает информацию о текущем авторизованном пользователе или 401 Unauthorized");
    }


}