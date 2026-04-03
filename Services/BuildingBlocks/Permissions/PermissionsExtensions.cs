namespace BuildingBlocks.Permissions;

public static class PermissionsExtensions
{
    public static void RequirePermissions(this AuthorizationPolicyBuilder builder, params string[] allowePermissions)
    {
        builder.AddRequirements(new PermissionsAuthorizationRequirement(allowePermissions));
    }
}
