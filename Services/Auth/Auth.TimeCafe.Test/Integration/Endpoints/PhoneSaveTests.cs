namespace Auth.TimeCafe.Test.Integration.Endpoints;

public class PhoneSaveTests(IntegrationApiFactory factory) : BaseEndpointTest(factory)
{
    private const string Endpoint = "/auth/account/phone";

    [Fact]
    public async Task Endpoint_SavePhone_Should_ReturnUnauthorized_WhenNoToken()
    {
        var dto = new { PhoneNumber = "+79123456789" };
        using var client = CreateClientWithDisabledRateLimiter();

        var response = await client.PostAsJsonAsync(Endpoint, dto);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Endpoint_SavePhone_Should_SavePhoneAndSetUnconfirmed_WhenValidRequest()
    {
        var (userId, accessToken) = await CreateAuthenticatedUserAsync();
        var dto = new { PhoneNumber = "+79123456789" };

        using var client = CreateClientWithDisabledRateLimiter();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await client.PostAsJsonAsync(Endpoint, dto);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var scope = Factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await userManager.FindByIdAsync(userId.ToString());

        user!.PhoneNumber.Should().Be("+79123456789");
        user.PhoneNumberConfirmed.Should().BeFalse();
    }

    [Fact]
    public async Task Endpoint_SavePhone_Should_ReturnOk_WhenPhoneAlreadyUnconfirmed()
    {
        var (userId, accessToken) = await CreateAuthenticatedUserAsync();
        using (var scope = Factory.Services.CreateScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var user = await userManager.FindByIdAsync(userId.ToString());
            user!.PhoneNumber = "+79123456789";
            user.PhoneNumberConfirmed = false;
            await userManager.UpdateAsync(user);
        }

        var dto = new { PhoneNumber = "+79123456789" };

        using var client = CreateClientWithDisabledRateLimiter();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await client.PostAsJsonAsync(Endpoint, dto);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var verifyScope = Factory.Services.CreateScope();
        var verifyManager = verifyScope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var updated = await verifyManager.FindByIdAsync(userId.ToString());

        updated!.PhoneNumber.Should().Be("+79123456789");
        updated.PhoneNumberConfirmed.Should().BeFalse();
    }
}
