using Microsoft.AspNetCore.Authorization;

namespace BuildingBlocks.Authorization;

public static class AuthorizationServiceExtensions
{
    public static IServiceCollection AddPermissionAuthorization(this IServiceCollection services)
    {
        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

        services.AddAuthorization(options =>
        {
            foreach (var permission in System.Enum.GetValues<Permission>())
            {
                options.AddPolicy($"Permission:{permission}", policy =>
                    policy.Requirements.Add(new PermissionRequirement(permission)));
            }
        });

        return services;
    }

    public static IServiceCollection AddPermissionAuthorization<TPermissionService>(
        this IServiceCollection services)
        where TPermissionService : class, IPermissionService
    {
        services.AddScoped<IPermissionService, TPermissionService>();
        return services.AddPermissionAuthorization();
    }

    public static IServiceCollection AddPermissionAuthorizationDev(this IServiceCollection services)
    {
        services.AddScoped<IPermissionService, AlwaysAllowPermissionService>();
        return services.AddPermissionAuthorization();
    }
}

internal class AlwaysAllowPermissionService : IPermissionService
{
    public Task<bool> HasPermissionAsync(Guid userId, Permission permission) => Task.FromResult(true);
    public Task<bool> HasAnyPermissionAsync(Guid userId, params Permission[] permissions) => Task.FromResult(true);
    public Task<bool> HasAllPermissionsAsync(Guid userId, params Permission[] permissions) => Task.FromResult(true);
}
