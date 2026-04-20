using BuildingBlocks.Contracts;

namespace BuildingBlocks.Authentication;

public sealed class UserContext(IHttpContextAccessor contextAccessor) : IUserContext
{
    public Guid UserId =>
        contextAccessor.HttpContext?.User?.GetUserId()
        ?? throw new InvalidOperationException("User id is unavailable");
}

internal static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal? principal)
    {
        var userId = principal?.FindFirst("sub")?.Value;

        return Guid.TryParse(userId, out var parsedUserId)
            ? parsedUserId
            : throw new InvalidOperationException("User id is unavailable");
    }
}
