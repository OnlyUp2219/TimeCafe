namespace BuildingBlocks.Authorization;

public enum Permission
{
    // Client (4 разрешения для клиента — свои данные)
    ClientView = 100,
    ClientEdit = 101,
    ClientDelete = 102,
    ClientCreate = 103,

    // Admin (4 разрешения + все Client = 8 у админа)
    AdminView = 200,
    AdminEdit = 201,
    AdminDelete = 202,
    AdminCreate = 203,
}
