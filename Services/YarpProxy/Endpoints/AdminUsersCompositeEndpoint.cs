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
            if (authResponse == null || authResponse.Items == null)
                return Results.Problem("Не удалось получить список пользователей из сервиса Auth");

            var userIds = authResponse.Items.Select(u => u.Id).ToList();

            List<ProfileDto>? profiles = null;
            List<BalanceDto>? balances = null;

            if (userIds.Count > 0)
            {
                var profilesTask = SafePostBatchAsync<List<ProfileDto>>(profileClient, "/userprofile/profiles/batch", userIds);
                var balancesTask = SafePostBatchAsync<List<BalanceDto>>(billingClient, "/billing/balance/batch", userIds);

                await Task.WhenAll(profilesTask, balancesTask);

                profiles = await profilesTask;
                balances = await balancesTask;
            }

            var profilesMap = profiles?.ToDictionary(p => p.UserId) ?? [];
            var balancesMap = balances?.ToDictionary(b => b.UserId) ?? [];

            var compositeUsers = authResponse.Items.Select(user => new
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
                Items = compositeUsers,
                authResponse.Metadata
            });
        })
        .WithTags("AdminComposite")
        .WithName("GetAdminUsersComposite")
        .WithSummary("Композитный список пользователей для админки")
        .RequireAuthorization(policy => policy.RequirePermissions(Permissions.AccountAdminRead));
    }

    private record AuthUsersResponse(List<AuthUserDto> Items, PageMetadataDto Metadata);
    private record AuthUserDto(Guid Id, string Email, string? Name, string Role, string Status, bool EmailConfirmed, bool PhoneNumberConfirmed, string? PhoneNumber);
    private record PageMetadataDto(int Page, int PageSize, int TotalCount, int TotalPages);

    private record ProfileDto(Guid UserId, string FirstName, string LastName, string? MiddleName, string? PhotoUrl, int ProfileStatus);
    private record BalanceDto(Guid UserId, decimal CurrentBalance, decimal Debt);

    private static async Task<T?> SafePostBatchAsync<T>(HttpClient client, string url, object body) where T : class
    {
        try
        {
            var response = await client.PostAsJsonAsync(url, body);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<T>();
            }
        }
        catch
        {
        }
        return null;
    }
}
