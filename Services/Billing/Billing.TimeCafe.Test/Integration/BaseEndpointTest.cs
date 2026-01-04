namespace Billing.TimeCafe.Test.Integration;

public abstract class BaseEndpointTest(IntegrationApiFactory factory) : IClassFixture<IntegrationApiFactory>
{
    protected HttpClient Client { get; } = factory.CreateClient();
    protected IntegrationApiFactory Factory { get; } = factory;

    protected HttpClient CreateClientWithConfig(IDictionary<string, string?> overrides)
    {
        return Factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(overrides));
        }).CreateClient();
    }
}
