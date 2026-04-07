namespace BuildingBlocks.Permissions;

public static class Permissions
{
    public const string AccountSelfRead = "auth.account.self.read";
    public const string AccountAdminRead = "auth.account.admin.read";

    public const string RbacRoleCreate = "auth.rbac.role.create";
    public const string RbacRoleRead = "auth.rbac.role.read";
    public const string RbacRoleUpdate = "auth.rbac.role.update";
    public const string RbacRoleDelete = "auth.rbac.role.delete";
    public const string RbacRoleClaimsUpdate = "auth.rbac.role.claims.update";
    public const string RbacPermissionRead = "auth.rbac.permission.read";
}
