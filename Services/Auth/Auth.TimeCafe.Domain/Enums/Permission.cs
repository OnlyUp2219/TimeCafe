namespace Auth.TimeCafe.Domain.Enums;

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
