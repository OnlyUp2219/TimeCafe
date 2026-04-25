namespace YarpProxy.Endpoints;

public class AdminUsersCompositeEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/admin/users-composite", async (
            [FromServices] IHttpClientFactory httpClientFactory,
            [FromQuery] int page = 1,
            [FromQuery] int size = 20,
            [FromQuery] string? search = null,
            [FromQuery] string? status = null) =>
        {
            var authClient = httpClientFactory.CreateClient("Auth");
            var profileClient = httpClientFactory.CreateClient("UserProfile");
            var billingClient = httpClientFactory.CreateClient("Billing");

            var authUrl = $"/auth/admin/users?page={page}&size={size}";
            if (!string.IsNullOrEmpty(search))
                authUrl += $"&search={search}";
            if (!string.IsNullOrEmpty(status))
                authUrl += $"&status={status}";

            var authResponse = await authClient.GetFromJsonAsync<AuthUsersResponse>(authUrl);
            if (authResponse == null || authResponse.Users == null)
                return Results.Problem("Не удалось получить список пользователей из сервиса Auth");

            var userIds = authResponse.Users.Select(u => u.Id).ToList();

            var profilesTask = profileClient.PostAsJsonAsync("/userprofile/profiles/batch", userIds);
            var balancesTask = billingClient.PostAsJsonAsync("/billing/balance/batch", userIds);

            await Task.WhenAll(profilesTask, balancesTask);

            var profilesResponse = await profilesTask;
            var balancesResponse = await balancesTask;

            List<ProfileDto>? profiles = null;
            if (profilesResponse.IsSuccessStatusCode)
            {
                profiles = await profilesResponse.Content.ReadFromJsonAsync<List<ProfileDto>>();
            }

            List<BalanceDto>? balances = null;
            if (balancesResponse.IsSuccessStatusCode)
            {
                balances = await balancesResponse.Content.ReadFromJsonAsync<List<BalanceDto>>();
            }

            var profilesMap = profiles?.ToDictionary(p => p.UserId) ?? new Dictionary<Guid, ProfileDto>();
            var balancesMap = balances?.ToDictionary(b => b.UserId) ?? new Dictionary<Guid, BalanceDto>();

            var compositeUsers = authResponse.Users.Select(user => new
            {
                user.Id,
                user.Email,
                user.Name,
                user.Role,
                user.Status,
                user.EmailConfirmed,
                user.PhoneNumberConfirmed,
                user.PhoneNumber,
                Profile = profilesMap.TryGetValue(user.Id, out var p) ? p : null,
                Balance = balancesMap.TryGetValue(user.Id, out var b) ? b : null
            }).ToList();

            return Results.Ok(new
            {
                Users = compositeUsers,
                authResponse.Pagination
            });
        })
        .WithTags("AdminComposite")
        .WithName("GetAdminUsersComposite")
        .WithSummary("Композитный список пользователей для админки")
        .RequireAuthorization(policy => policy.RequirePermissions(Permissions.AccountAdminRead));
    }

    private record AuthUsersResponse(List<AuthUserDto> Users, PaginationDto Pagination);
    private record AuthUserDto(Guid Id, string Email, string? Name, string Role, string Status, bool EmailConfirmed, bool PhoneNumberConfirmed, string? PhoneNumber);
    private record PaginationDto(int CurrentPage, int PageSize, int TotalCount, int TotalPages);

    private record ProfileDto(Guid UserId, string FirstName, string LastName, string? MiddleName, string? PhotoUrl, int ProfileStatus);
    private record BalanceDto(Guid UserId, decimal CurrentBalance, decimal Debt);
}
