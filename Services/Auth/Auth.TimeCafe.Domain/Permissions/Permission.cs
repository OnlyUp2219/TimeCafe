namespace Auth.TimeCafe.Domain.Permissions;

public enum Permission
{
    // Клиентские разрешения
    ClientView,
    ClientEdit,
    ClientDelete,
    ClientCreate,
    // Админские разрешения
    AdminView,
    AdminEdit,
    AdminDelete,
    AdminCreate,
}
