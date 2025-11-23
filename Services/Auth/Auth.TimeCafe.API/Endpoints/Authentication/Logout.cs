namespace Auth.TimeCafe.API.Endpoints.Authentication;

public class Logout : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/logout", async (
            HttpContext context,
            ISender sender) =>
        {
            const string cookieName = "refresh_token";
            if (!context.Request.Cookies.TryGetValue(cookieName, out var refresh) || string.IsNullOrWhiteSpace(refresh))
            {
                AppendExpiredCookie(context, cookieName);
                return Results.Ok(new { message = "Refresh cookie отсутствует", revoked = false });
            }

            var command = new LogoutCommand(refresh);
            var result = await sender.Send(command);

            AppendExpiredCookie(context, cookieName);

            return result.ToHttpResultV2(r =>
                Results.Ok(new { message = r.Message ?? "Выход выполнен", revoked = r.Revoked }));
        })
            .WithTags("Authentication")
            .WithName("Logout")
            .WithSummary("Выход пользователя и отзыв refresh-токена")
            .WithDescription("Выход пользователя из системы (v2). Берёт refresh из httpOnly cookie, отзывает и удаляет cookie. Возвращает сообщение и статус отзыва.");
    }

    private static void AppendExpiredCookie(HttpContext context, string cookieName)
    {
        context.Response.Cookies.Append(cookieName, string.Empty,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Path = "/",
                Expires = DateTimeOffset.UtcNow.AddDays(-1)
            });
    }
}


