using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using BuildingBlocks.Permissions;
using Microsoft.Extensions.Caching.Hybrid;
using PermissionKeys = BuildingBlocks.Permissions.Permissions;

namespace Auth.TimeCafe.Test.Unit.Permissions;

public class PermissionClaimsTransformerTests
{
    [Fact]
    public async Task Transformer_Should_AddPermissionClaims_FromRoleClaims()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"perm_transformer_{Guid.NewGuid():N}")
            .Options;

        await using var context = new ApplicationDbContext(options);

        var role = new IdentityRole<Guid>(Roles.Client)
        {
            NormalizedName = Roles.Client.ToUpperInvariant()
        };

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "perm.transformer@example.com",
            Email = "perm.transformer@example.com",
            NormalizedUserName = "PERM.TRANSFORMER@EXAMPLE.COM",
            NormalizedEmail = "PERM.TRANSFORMER@EXAMPLE.COM",
            EmailConfirmed = true
        };

        context.Roles.Add(role);
        context.Users.Add(user);
        context.UserRoles.Add(new IdentityUserRole<Guid> { UserId = user.Id, RoleId = role.Id });
        context.RoleClaims.Add(new IdentityRoleClaim<Guid>
        {
            RoleId = role.Id,
            ClaimType = CustomClaimTypes.Permissions,
            ClaimValue = PermissionKeys.AccountSelfRead
        });

        await context.SaveChangesAsync();

        var services = new ServiceCollection();
#pragma warning disable EXTEXP0018
        services.AddHybridCache();
#pragma warning restore EXTEXP0018
        await using var provider = services.BuildServiceProvider();
        var cache = provider.GetRequiredService<HybridCache>();

        var transformer = new PermissionClaimsTransformer(context, cache);

        var identity = new ClaimsIdentity(
        [
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(ClaimTypes.Role, Roles.Client)
        ],
        "TestAuth");

        var principal = new ClaimsPrincipal(identity);

        var transformed = await transformer.TransformAsync(principal);

        transformed.HasClaim(c => c.Type == CustomClaimTypes.Permissions && c.Value == PermissionKeys.AccountSelfRead)
            .Should().BeTrue();
    }
}