using Auth.TimeCafe.Infrastructure.Services;
using Carter;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Auth.TimeCafe.API.Endpoints;

public record RegisterDto(string Username, string Email, string Password);
public record LoginDto(string Email, string Password);
public record JwtRefreshRequest(string RefreshToken);
public record class TokensDto(string AccessToken, string RefreshToken);


public class CreateRegistry : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/registerWithUsername", async (
            UserManager<IdentityUser> userManager,
            IJwtService jwtService,
            [FromBody] RegisterDto dto) =>
        {
            var user = new IdentityUser
            {
                UserName = dto.Username,
                Email = dto.Email
            };

            var result = await userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                return Results.BadRequest(new { errors = result.Errors });

            var tokens = await jwtService.GenerateTokens(user);

            return Results.Ok(new TokensDto(tokens.AccessToken, tokens.RefreshToken));
        });

        app.MapPost("/login-jwt", async (
            UserManager<IdentityUser> userManager,
            IJwtService jwtService,
            HttpContext context,
            [FromBody] LoginDto dto) =>
        {
            var user = await userManager.FindByEmailAsync(dto.Email);
            if (user == null || !await userManager.CheckPasswordAsync(user, dto.Password))
                return Results.BadRequest(new
                { errors = new[] { new { code = "InvalidCredentials", description = "Неверный email или пароль" } } });

            var tokens = await jwtService.GenerateTokens(user);
            context.Response.Cookies.Append("Access-Token", tokens.AccessToken);

            return Results.Ok(new TokensDto(tokens.AccessToken, tokens.RefreshToken));
        });

        app.MapPost("/refresh-token-jwt", async (
            IJwtService jwtService,
            HttpContext context,
            [FromBody] JwtRefreshRequest request) =>
        {
            var tokens = await jwtService.RefreshTokens(request.RefreshToken);
            if (tokens == null) return Results.Unauthorized();
            context.Response.Cookies.Append("Access-Token", tokens.AccessToken);

            return Results.Ok(new TokensDto(tokens.AccessToken, tokens.RefreshToken));
        });

        app.MapGet("/protected-test", [Microsoft.AspNetCore.Authorization.Authorize]
        async (UserManager<IdentityUser> userManager, System.Security.Claims.ClaimsPrincipal user) =>
        {
            var userId = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Results.Unauthorized();
            var u = await userManager.FindByIdAsync(userId);
            return Results.Ok($"Protected OK. User: {u?.Email} ({userId})");
        });
    }
}
