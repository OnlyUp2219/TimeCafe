using BuildingBlocks.Authorization;
using System.Security.Claims;

namespace UserProfile.TimeCafe.API.Endpoints.Test;

public class AuthorizationTestEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/test/auth")
            .WithTags("AuthorizationTests")
            .WithDescription("Тестовые endpoints для проверки системы авторизации")
            .RequireAuthorization();

        group.MapGet("/admin-only", (HttpContext ctx) =>
        {
            var userId = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Results.Ok(new
            {
                message = "Доступ к админскому ресурсу разрешён",
                userId,
                endpoint = "admin-only",
                requiredPermission = Permission.AdminView.ToString()
            });
        })
        .WithName("TestAdminOnly")
        .WithSummary("Только для админов (AdminView)")
        .RequirePermission(Permission.AdminView);

        group.MapGet("/client-or-admin", (HttpContext ctx) =>
        {
            var userId = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Results.Ok(new
            {
                message = "Доступ разрешён клиенту или админу",
                userId,
                endpoint = "client-or-admin",
                requiredPermissions = new[] { Permission.ClientView.ToString(), Permission.AdminView.ToString() }
            });
        })
        .WithName("TestClientOrAdmin")
        .WithSummary("Для клиентов или админов (ClientView OR AdminView)")
        .RequirePermission(Permission.ClientView, Permission.AdminView);

        group.MapGet("/multi-permission", (HttpContext ctx) =>
        {
            var userId = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Results.Ok(new
            {
                message = "Доступ разрешён — есть все нужные разрешения",
                userId,
                endpoint = "multi-permission",
                requiredPermissions = new[] { Permission.ClientView.ToString(), Permission.ClientEdit.ToString() }
            });
        })
        .WithName("TestMultiPermission")
        .WithSummary("Требует оба разрешения (ClientView AND ClientEdit)")
        .RequireAllPermissions(Permission.ClientView, Permission.ClientEdit);

        group.MapGet("/profile/{userId:guid}", (Guid userId, HttpContext ctx) =>
        {
            var currentUserId = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isOwner = currentUserId == userId.ToString();
            
            return Results.Ok(new
            {
                message = isOwner 
                    ? "Просмотр своего профиля" 
                    : "Просмотр чужого профиля (у вас есть разрешение)",
                currentUserId,
                requestedUserId = userId,
                isOwner,
                endpoint = "profile/{userId}",
                idorProtection = "userId = currentUser OR Permission.ClientView"
            });
        })
        .WithName("TestIdorProfile")
        .WithSummary("IDOR защита: свой профиль ИЛИ разрешение ClientView")
        .RequireOwnerOrPermission("userId", Permission.ClientView);

        group.MapPut("/profile/{userId:guid}", (Guid userId, HttpContext ctx) =>
        {
            var currentUserId = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isOwner = currentUserId == userId.ToString();
            
            return Results.Ok(new
            {
                message = isOwner 
                    ? "Редактирование своего профиля разрешено" 
                    : "Редактирование чужого профиля (у вас есть разрешение ClientEdit)",
                currentUserId,
                requestedUserId = userId,
                isOwner,
                endpoint = "PUT profile/{userId}",
                idorProtection = "userId = currentUser OR Permission.ClientEdit"
            });
        })
        .WithName("TestIdorProfileEdit")
        .WithSummary("IDOR защита редактирования: свой профиль ИЛИ разрешение ClientEdit")
        .RequireOwnerOrPermission("userId", Permission.ClientEdit);

        group.MapPost("/admin-create", (HttpContext ctx) =>
        {
            var userId = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Results.Ok(new
            {
                message = "Админское создание разрешено",
                userId,
                endpoint = "POST admin-create",
                requiredPermission = Permission.AdminCreate.ToString()
            });
        })
        .WithName("TestAdminCreate")
        .WithSummary("Создание через админа (AdminCreate)")
        .RequirePermission(Permission.AdminCreate);

        group.MapGet("/public-authenticated", (HttpContext ctx) =>
        {
            var userId = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var roles = ctx.User.FindAll(ClaimTypes.Role).Select(c => c.Value);
            
            return Results.Ok(new
            {
                message = "Публичный endpoint — только JWT авторизация",
                userId,
                roles,
                endpoint = "public-authenticated",
                requiredPermission = "none (JWT only)"
            });
        })
        .WithName("TestPublicAuthenticated")
        .WithSummary("Требует только JWT токен без проверки разрешений");

        group.MapGet("/whoami", async (HttpContext ctx, IPermissionService permissionService) =>
        {
            var userId = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var roles = ctx.User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToArray();
            
            if (!Guid.TryParse(userId, out var userGuid))
            {
                return Results.BadRequest("Invalid userId in token");
            }

            var allPermissions = Enum.GetValues<Permission>();
            var userPermissions = new List<string>();
            
            foreach (var perm in allPermissions)
            {
                if (await permissionService.HasPermissionAsync(userGuid, perm))
                {
                    userPermissions.Add(perm.ToString());
                }
            }

            return Results.Ok(new
            {
                userId,
                roles,
                permissions = userPermissions,
                message = "Информация о текущем пользователе"
            });
        })
        .WithName("TestWhoAmI")
        .WithSummary("Показывает userId, роли и все разрешения текущего пользователя");
    }
}
