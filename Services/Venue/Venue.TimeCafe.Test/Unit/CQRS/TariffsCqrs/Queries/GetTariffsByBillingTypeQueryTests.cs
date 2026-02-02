namespace Venue.TimeCafe.Test.Unit.CQRS.TariffsCqrs.Queries;

public class GetTariffsByBillingTypeQueryTests : BaseCqrsHandlerTest
{
    private readonly GetTariffsByBillingTypeQueryHandler _handler;

    public GetTariffsByBillingTypeQueryTests()
    {
        _handler = new GetTariffsByBillingTypeQueryHandler(TariffRepositoryMock.Object);
    }

    [Fact]
    public async Task Handler_Should_ReturnSuccess_WhenTariffsFound()
    {
        var query = new GetTariffsByBillingTypeQuery(BillingType.PerMinute);
        var tariffs = new List<TariffWithThemeDto>
        {
            new() { TariffId = Guid.NewGuid(), Name = "Tariff 1", PricePerMinute = 10m, BillingType = BillingType.PerMinute },
            new() { TariffId = Guid.NewGuid(), Name = "Tariff 2", PricePerMinute = 20m, BillingType = BillingType.PerMinute }
        };

        TariffRepositoryMock.Setup(r => r.GetByBillingTypeAsync(BillingType.PerMinute)).ReturnsAsync(tariffs);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Tariffs.Should().NotBeNull();
        result.Tariffs.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handler_Should_ReturnSuccess_WhenNoTariffsForType()
    {
        var query = new GetTariffsByBillingTypeQuery(BillingType.Hourly);
        var tariffs = new List<TariffWithThemeDto>();

        TariffRepositoryMock.Setup(r => r.GetByBillingTypeAsync(BillingType.Hourly)).ReturnsAsync(tariffs);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Tariffs.Should().NotBeNull();
        result.Tariffs.Should().BeEmpty();
    }

    [Fact]
    public async Task Handler_Should_ThrowCqrsResultException_WhenExceptionThrown()
    {
        var query = new GetTariffsByBillingTypeQuery(BillingType.PerMinute);

        TariffRepositoryMock.Setup(r => r.GetByBillingTypeAsync(BillingType.PerMinute)).ThrowsAsync(new Exception());

        var ex = await Assert.ThrowsAsync<CqrsResultException>(
            () => _handler.Handle(query, CancellationToken.None));

        ex.Result.Should().NotBeNull();
        ex.Result!.Success.Should().BeFalse();
        ex.Result.Code.Should().Be("GetTariffsByBillingTypeFailed");
        ex.Result.StatusCode.Should().Be(500);
    }
}
