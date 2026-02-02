namespace Venue.TimeCafe.Test.Unit.CQRS.TariffsCqrs.Queries;

public class GetActiveTariffsQueryTests : BaseCqrsHandlerTest
{
    private readonly GetActiveTariffsQueryHandler _handler;

    public GetActiveTariffsQueryTests()
    {
        _handler = new GetActiveTariffsQueryHandler(TariffRepositoryMock.Object);
    }

    [Fact]
    public async Task Handler_Should_ReturnSuccess_WhenActiveTariffsFound()
    {
        var query = new GetActiveTariffsQuery();
        var tariffs = new List<TariffWithThemeDto>
        {
            new() { TariffId = Guid.NewGuid(), Name = "Tariff 1", PricePerMinute = 10m, IsActive = true },
            new() { TariffId = Guid.NewGuid(), Name = "Tariff 2", PricePerMinute = 20m, IsActive = true }
        };

        TariffRepositoryMock.Setup(r => r.GetActiveAsync()).ReturnsAsync(tariffs);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Tariffs.Should().NotBeNull();
        result.Tariffs.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handler_Should_ReturnSuccess_WhenNoActiveTariffs()
    {
        var query = new GetActiveTariffsQuery();
        var tariffs = new List<TariffWithThemeDto>();

        TariffRepositoryMock.Setup(r => r.GetActiveAsync()).ReturnsAsync(tariffs);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Tariffs.Should().NotBeNull();
        result.Tariffs.Should().BeEmpty();
    }

    [Fact]
    public async Task Handler_Should_ThrowCqrsResultException_WhenExceptionThrown()
    {
        var query = new GetActiveTariffsQuery();

        TariffRepositoryMock.Setup(r => r.GetActiveAsync()).ThrowsAsync(new Exception());

        var ex = await Assert.ThrowsAsync<CqrsResultException>(
            () => _handler.Handle(query, CancellationToken.None));

        ex.Result.Should().NotBeNull();
        ex.Result!.Success.Should().BeFalse();
        ex.Result.Code.Should().Be("GetActiveTariffsFailed");
        ex.Result.StatusCode.Should().Be(500);
    }
}
