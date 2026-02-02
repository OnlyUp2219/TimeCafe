namespace Venue.TimeCafe.Test.Unit.CQRS.TariffsCqrs.Queries;

public class GetAllTariffsQueryTests : BaseCqrsHandlerTest
{
    private readonly GetAllTariffsQueryHandler _handler;

    public GetAllTariffsQueryTests()
    {
        _handler = new GetAllTariffsQueryHandler(TariffRepositoryMock.Object);
    }

    [Fact]
    public async Task Handler_Should_ReturnSuccess_WhenTariffsFound()
    {
        var query = new GetAllTariffsQuery();
        var tariffs = new List<TariffWithThemeDto>
        {
            new() { TariffId = Guid.NewGuid(), Name = "Tariff 1", PricePerMinute = 10m },
            new() { TariffId = Guid.NewGuid(), Name = "Tariff 2", PricePerMinute = 20m }
        };

        TariffRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(tariffs);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Tariffs.Should().NotBeNull();
        result.Tariffs.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handler_Should_ReturnSuccess_WhenNoTariffs()
    {
        var query = new GetAllTariffsQuery();
        var tariffs = new List<TariffWithThemeDto>();

        TariffRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(tariffs);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Tariffs.Should().NotBeNull();
        result.Tariffs.Should().BeEmpty();
    }

    [Fact]
    public async Task Handler_Should_ThrowCqrsResultException_WhenExceptionThrown()
    {
        var query = new GetAllTariffsQuery();

        TariffRepositoryMock.Setup(r => r.GetAllAsync()).ThrowsAsync(new Exception());

        var ex = await Assert.ThrowsAsync<CqrsResultException>(
            () => _handler.Handle(query, CancellationToken.None));

        ex.Result.Should().NotBeNull();
        ex.Result!.Success.Should().BeFalse();
        ex.Result.Code.Should().Be("GetTariffsFailed");
        ex.Result.StatusCode.Should().Be(500);
    }
}
