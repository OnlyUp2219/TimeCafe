using System.Security.Claims;
using Auth.TimeCafe.Application.CQRS.Auth.Commands;
using MediatR;

namespace Auth.TimeCafe.API.Endpoints;

public class CreateRegistry : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/registerWithUsername", async (
            IMediator mediator,
            [FromBody] RegisterDto dto) =>
        {
            var command = new RegisterCommand(dto.Username, dto.Email, dto.Password, SendEmail: true);
            var result = await mediator.Send(command);

            if (!result.IsSuccess)
                return Results.BadRequest(new { errors = result.Errors });

            return Results.Ok(new { message = result.Data });
        })
            .WithTags("Authentication")
            .WithName("Register");

        app.MapPost("/registerWithUsername-mock", async (
            IMediator mediator,
            [FromBody] RegisterDto dto) =>
        {
            var command = new RegisterCommand(dto.Username, dto.Email, dto.Password, SendEmail: false);
            var result = await mediator.Send(command);

            if (!result.IsSuccess)
                return Results.BadRequest(new { errors = result.Errors });

            return Results.Ok(new { message = result.Data!.Message, confirmLink = result.Data.ConfirmLink });
        })
            .WithTags("Authentication")
            .WithName("RegisterMock")
            .WithSummary("Mock: Возвращает ссылку подтверждения без отправки email");

        app.MapPost("/login-jwt", async (
            IMediator mediator,
            HttpContext context,
            [FromBody] LoginDto dto) =>
        {
            var command = new LoginCommand(dto.Email, dto.Password);
            var result = await mediator.Send(command);

            if (!result.IsSuccess)
                return Results.BadRequest(new { errors = result.Errors });

#if DEBUG
            context.Response.Cookies.Append("Access-Token", result.Data!.AccessToken);
#endif

            return Results.Ok(result.Data);
        })
            .WithTags("Authentication")
            .WithName("Login");

        app.MapPost("/refresh-token-jwt", async (
            IMediator mediator,
            HttpContext context,
            [FromBody] JwtRefreshRequest request) =>
        {
            var command = new RefreshTokenCommand(request.RefreshToken);
            var result = await mediator.Send(command);

            if (!result.IsSuccess)
                return Results.Unauthorized();

            context.Response.Cookies.Append("Access-Token", result.Data!.AccessToken);

            return Results.Ok(result.Data);
        })
            .WithTags("Authentication")
            .WithName("RefreshToken");

        app.MapGet("/protected-test",
        async (
            UserManager<IdentityUser> userManager,
            ClaimsPrincipal user) =>
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Results.Unauthorized();
            var u = await userManager.FindByIdAsync(userId);
            return Results.Ok($"Protected OK. User: {u?.Email} ({userId})");
        })
            .RequireAuthorization()
            .WithTags("Authentication")
            .WithName("Test401");


        app.MapPost("/logout", async (
            [FromBody] LogoutRequest request,
            IMediator mediator,
            ClaimsPrincipal principal) =>
        {
            var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var command = new LogoutCommand(request.RefreshToken, userId);
            var result = await mediator.Send(command);

            if (!result.IsSuccess)
            {
                if (result.Errors.Contains("Unauthorized"))
                    return Results.Unauthorized();
                return Results.BadRequest(new { errors = result.Errors });
            }

            return Results.Ok(new { message = "Logged out", revoked = result.Data });
        })
            .RequireAuthorization()
            .WithTags("Authentication")
            .WithName("Logout");

    }
}

public record class LogoutRequest(string RefreshToken);
public record JwtRefreshRequest(string RefreshToken);
