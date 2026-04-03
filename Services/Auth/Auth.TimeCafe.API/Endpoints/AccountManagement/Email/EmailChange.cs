using Auth.TimeCafe.Application.CQRS.Account.Commands;

using System.Security.Claims;

namespace Auth.TimeCafe.API.Endpoints.AccountManagement;

public record ChangeEmailRequest(
    /// <example>newemail@example.com</example>
    string NewEmail);
public record ConfirmChangeEmailRequest(
    /// <example>550e8400-e29b-41d4-a716-446655440000</example>
    string UserId,
    /// <example>newemail@example.com</example>
    string NewEmail,
    /// <example>Q2ZESjhOVGtMWUZ5...</example>
    string Token);

public class EmailChange : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/email").WithTags("EmailConfirmation");

        group.MapPost("/change", async (
            [FromBody] ChangeEmailRequest request,
            ClaimsPrincipal principal,
            [FromServices] ISender sender) =>
        {
            var userId = principal.FindFirstValue("sub");
            if (userId == null)
                return Results.Unauthorized();

            var command = new RequestEmailChangeCommand(userId, request.NewEmail, SendEmail: true);
            var result = await sender.Send(command);

            return result.ToHttpResult(onSuccess: r => Results.Ok(new { message = r.Message }));
        })
        .RequireAuthorization()
        .RequireRateLimiting("OneRequestPerInterval")
        .RequireRateLimiting("MaxRequestPerWindow")
        .WithName("RequestEmailChange")
        .WithSummary("Запрос на смену email")
        .Produces(200)
        .WithDescription("Отправляет ссылку для подтверждения смены email на новый адрес.");

        group.MapPost("/change-mock", async (
            [FromBody] ChangeEmailRequest request,
            ClaimsPrincipal principal,
            [FromServices] ISender sender) =>
        {
            var userId = principal.FindFirstValue("sub");
            if (userId == null)
                return Results.Unauthorized();

            var command = new RequestEmailChangeCommand(userId, request.NewEmail, SendEmail: false);
            var result = await sender.Send(command);

            return result.ToHttpResult(onSuccess: r => Results.Ok(new { callbackUrl = r.CallbackUrl }));
        })
        .RequireAuthorization()
        .RequireRateLimiting("OneRequestPerInterval")
        .RequireRateLimiting("MaxRequestPerWindow")
        .WithName("RequestEmailChangeMock")
        .WithSummary("Mock: запрос на смену email")
        .Produces(200)
        .WithDescription("Тестовый endpoint: возвращает callbackUrl для подтверждения смены email без реальной отправки письма.");

        group.MapPost("/change-confirm", async (
            [FromBody] ConfirmChangeEmailRequest request,
            [FromServices] ISender sender) =>
        {
            var command = new ConfirmEmailChangeCommand(request.UserId, request.NewEmail, request.Token);
            var result = await sender.Send(command);

            return result.ToHttpResult(onSuccess: r => Results.Ok(new { message = r.Message }));
        })
        .WithName("ConfirmEmailChange")
        .WithSummary("Подтверждение смены email")
        .Produces(200)
        .WithDescription("Подтверждает смену email по токену, полученному по ссылке.");
    }
}
