namespace BuildingBlocks.Permissions;

public class PermissionsAuthorizationRequirement(params string[] allowedPermissions) :
    AuthorizationHandler<PermissionsAuthorizationRequirement>, IAuthorizationRequirement
{
    public string[] AllowedPermissions { get; } = allowedPermissions;

    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionsAuthorizationRequirement requirement)
    {
        foreach (var permission in requirement.AllowedPermissions)
        {
            var found = context.User.FindFirst(c =>
            c.Type == CustomClaimTypes.Permissions && c.Value == permission) is not null;

            if (found)
            {
                context.Succeed(requirement);
                break;
            }
        }

        return Task.CompletedTask;
    }
}
