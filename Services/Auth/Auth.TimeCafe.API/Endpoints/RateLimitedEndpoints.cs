namespace Auth.TimeCafe.API.Endpoints;

public class RateLimitedEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/test-rate-limit", (HttpContext context, RateLimitConfig cfg) =>
        {

            return Results.Ok(new
            {
                success = true,
                time = DateTime.UtcNow
            });
        })
        .RequireRateLimiting("OneRequestPerInterval")
        .RequireRateLimiting("MaxRequestPerWindow")
        .WithTags("RateLimit")
        .WithName("TestRateLimit");

        app.MapGet("/api/test-rate-limit2", (HttpContext context, RateLimitConfig cfg) =>
        {
            context.Response.Headers["X-Rate-Limit-Window"] = cfg.MinIntervalSeconds.ToString();

            return Results.Ok(new
            {
                success = true,
                time = DateTime.UtcNow
            });
        })
        .RequireRateLimiting("OneRequestPerInterval")
        .RequireRateLimiting("MaxRequestPerWindow")
        .WithTags("RateLimit")
        .WithName("TestRateLimit2");
    }
}