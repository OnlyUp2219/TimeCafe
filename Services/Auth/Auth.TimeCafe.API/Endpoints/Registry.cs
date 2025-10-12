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
            [FromBody] LoginDto dto) =>
        {
            var user = await userManager.FindByEmailAsync(dto.Email);
            if (user == null || !await userManager.CheckPasswordAsync(user, dto.Password))
                return Results.BadRequest(new
                    { errors = new[]{ new { code = "InvalidCredentials", description = "Неверный email или пароль" }}});

            var tokens = await jwtService.GenerateTokens(user);
            return Results.Ok(new TokensDto(tokens.AccessToken, tokens.RefreshToken));
        });

        app.MapPost("/refresh-token-jwt", async (
            IJwtService jwtService,
            [FromBody] JwtRefreshRequest request) =>
        {
            var tokens = await jwtService.RefreshTokens(request.RefreshToken);
            if (tokens == null) return Results.Unauthorized();

            return Results.Ok(new TokensDto(tokens.AccessToken, tokens.RefreshToken));
        });
    }
}
