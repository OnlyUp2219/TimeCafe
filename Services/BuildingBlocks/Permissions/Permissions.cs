namespace BuildingBlocks.Permissions;

public static class Permissions
{
    public const string AccountSelfRead = "auth.account.self.read";
    public const string AccountAdminRead = "auth.account.admin.read";
    public const string AccountEmailChange = "auth.account.email.change";
    public const string AccountPasswordChange = "auth.account.password.change";
    public const string AccountPhoneSave = "auth.account.phone.save";
    public const string AccountPhoneClear = "auth.account.phone.clear";
    public const string AccountPhoneGenerate = "auth.account.phone.generate";
    public const string AccountPhoneVerify = "auth.account.phone.verify";
    public const string AccountPhoneStatusRead = "auth.account.phone.status.read";

    public const string DebugRateLimitRead = "auth.debug.ratelimit.read";
    public const string DebugProtectedRead = "auth.debug.protected.read";

    public const string RbacRoleCreate = "auth.rbac.role.create";
    public const string RbacRoleRead = "auth.rbac.role.read";
    public const string RbacRoleUpdate = "auth.rbac.role.update";
    public const string RbacRoleDelete = "auth.rbac.role.delete";
    public const string RbacRoleClaimsUpdate = "auth.rbac.role.claims.update";
    public const string RbacPermissionRead = "auth.rbac.permission.read";
    public const string RbacUserRoleAssign = "auth.rbac.userrole.assign";
    public const string RbacUserRoleRemove = "auth.rbac.userrole.remove";
}
