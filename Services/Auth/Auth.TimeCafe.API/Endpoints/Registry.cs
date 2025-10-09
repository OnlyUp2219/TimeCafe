using Carter;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Auth.TimeCafe.API.Endpoints;

public record RegisterDto(string UserName, string Email, string Password);
public class CreateRegistry : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
{
    app.MapPost("/registerWithUsername", async (
    UserManager<IdentityUser> userManager,
    [FromBody] RegisterDto dto) =>
    {
        var user = new IdentityUser
        {
            UserName = dto.UserName,
            Email = dto.Email
        };

        var result = await userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
            return Results.BadRequest(new { errors = result.Errors });

        return Results.Ok();
    });
}
}