namespace BuildingBlocks.Permissions;

public static class Roles
{
    public const string Client = "Client";
    public const string Admin = "Admin";
    public const string SuperAdmin = "SuperAdmin";
    public const string Manager = "Manager";
    public const string Receptionist = "Receptionist";

    public static bool IsSystemRole(string roleName)
    {
        return string.Equals(roleName, Admin, StringComparison.OrdinalIgnoreCase)
               || string.Equals(roleName, Client, StringComparison.OrdinalIgnoreCase)
               || string.Equals(roleName, SuperAdmin, StringComparison.OrdinalIgnoreCase)
               || string.Equals(roleName, Manager, StringComparison.OrdinalIgnoreCase)
               || string.Equals(roleName, Receptionist, StringComparison.OrdinalIgnoreCase);
    }
}
