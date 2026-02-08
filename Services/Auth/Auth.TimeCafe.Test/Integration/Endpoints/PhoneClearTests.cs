namespace Auth.TimeCafe.Test.Integration.Endpoints;

public class PhoneClearTests(IntegrationApiFactory factory) : BaseEndpointTest(factory)
{
    private const string Endpoint = "/auth/account/phone";

    [Fact]
    public async Task Endpoint_ClearPhone_Should_ReturnUnauthorized_WhenNoToken()
    {
        using var client = CreateClientWithDisabledRateLimiter();

        var response = await client.DeleteAsync(Endpoint);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Endpoint_ClearPhone_Should_RemovePhone_WhenExists()
    {
        var (userId, accessToken) = await CreateAuthenticatedUserAsync();
        using (var scope = Factory.Services.CreateScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var user = await userManager.FindByIdAsync(userId.ToString());
            user!.PhoneNumber = "+79123456789";
            user.PhoneNumberConfirmed = true;
            await userManager.UpdateAsync(user);
        }

        using var client = CreateClientWithDisabledRateLimiter();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await client.DeleteAsync(Endpoint);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var verifyScope = Factory.Services.CreateScope();
        var verifyManager = verifyScope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var updated = await verifyManager.FindByIdAsync(userId.ToString());

        updated!.PhoneNumber.Should().BeNull();
        updated.PhoneNumberConfirmed.Should().BeFalse();
    }
}
