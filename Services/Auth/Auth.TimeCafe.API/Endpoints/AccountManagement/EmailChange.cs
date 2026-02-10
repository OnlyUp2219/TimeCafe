using Auth.TimeCafe.API.DTOs;
using Auth.TimeCafe.Application.CQRS.Account.Commands;

using System.Security.Claims;

namespace Auth.TimeCafe.API.Endpoints.AccountManagement;

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
            var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Results.Unauthorized();

            var command = new RequestEmailChangeCommand(userId, request.NewEmail, SendEmail: true);
            var result = await sender.Send(command);

            return result.ToHttpResultV2(onSuccess: r =>
            {
                return Results.Ok(new { message = r.Message });
            });
        })
        .RequireAuthorization()
        .RequireRateLimiting("OneRequestPerInterval")
        .RequireRateLimiting("MaxRequestPerWindow")
        .WithName("RequestEmailChange")
        .WithSummary("Запрос на смену email")
        .WithDescription("Отправляет ссылку для подтверждения смены email на новый адрес.");

        group.MapPost("/change-mock", async (
            [FromBody] ChangeEmailRequest request,
            ClaimsPrincipal principal,
            [FromServices] ISender sender) =>
        {
            var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Results.Unauthorized();

            var command = new RequestEmailChangeCommand(userId, request.NewEmail, SendEmail: false);
            var result = await sender.Send(command);

            return result.ToHttpResultV2(onSuccess: r =>
            {
                return Results.Ok(new { callbackUrl = r.CallbackUrl });
            });
        })
        .RequireAuthorization()
        .RequireRateLimiting("OneRequestPerInterval")
        .RequireRateLimiting("MaxRequestPerWindow")
        .WithName("RequestEmailChangeMock")
        .WithSummary("Mock: запрос на смену email")
        .WithDescription("Тестовый endpoint: возвращает callbackUrl для подтверждения смены email без реальной отправки письма.");

        group.MapPost("/change-confirm", async (
            [FromBody] ConfirmChangeEmailRequest request,
            [FromServices] ISender sender) =>
        {
            var command = new ConfirmEmailChangeCommand(request.UserId, request.NewEmail, request.Token);
            var result = await sender.Send(command);

            return result.ToHttpResultV2(onSuccess: r =>
            {
                return Results.Ok(new { message = r.Message });
            });
        })
        .WithName("ConfirmEmailChange")
        .WithSummary("Подтверждение смены email")
        .WithDescription("Подтверждает смену email по токену, полученному по ссылке.");
    }
}
