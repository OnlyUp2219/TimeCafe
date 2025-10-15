using System.Security.Claims;

namespace Auth.TimeCafe.API.Endpoints;

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
        })
            .WithTags("Authentication")
            .WithName("Register");

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

            #if DEBUG
            context.Response.Cookies.Append("Access-Token", tokens.AccessToken);
            #endif


            return Results.Ok(new TokensDto(tokens.AccessToken, tokens.RefreshToken));
        })
            .WithTags("Authentication")
            .WithName("Login");

        app.MapPost("/refresh-token-jwt", async (
            IJwtService jwtService,
            HttpContext context,
            [FromBody] JwtRefreshRequest request) =>
        {
            var tokens = await jwtService.RefreshTokens(request.RefreshToken);
            if (tokens == null) return Results.Unauthorized();
            context.Response.Cookies.Append("Access-Token", tokens.AccessToken);

            return Results.Ok(new TokensDto(tokens.AccessToken, tokens.RefreshToken));
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

    }
}

