using System.Security.Claims;

namespace BuildingBlocks.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal principal)
    {
        var value = principal.FindFirstValue("sub")
            ?? principal.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? principal.FindFirstValue("nameid");

        if (string.IsNullOrEmpty(value) || !Guid.TryParse(value, out var userId))
        {
            throw new UnauthorizedAccessException("Идентификатор пользователя не найден в claims.");
        }

        return userId;
    }

    public static Guid? TryGetUserId(this ClaimsPrincipal principal)
    {
        var value = principal.FindFirstValue("sub")
            ?? principal.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? principal.FindFirstValue("nameid");

        return Guid.TryParse(value, out var userId) ? userId : null;
    }
}
