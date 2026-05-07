

namespace Venue.TimeCafe.Test.Unit.CQRS.TariffsCqrs.Queries;

public class GetTariffByIdQueryTests : BaseCqrsHandlerTest
{
    private readonly GetTariffByIdQueryHandler _handler;

    public GetTariffByIdQueryTests()
    {
        _handler = new GetTariffByIdQueryHandler(UowMock.Object);
    }

    [Fact]
    public async Task Handler_Should_ReturnSuccess_WhenTariffFound()
    {
        var tariffId = Guid.NewGuid();
        var query = new GetTariffByIdQuery(tariffId);
        var tariff = new TariffWithThemeDto
        {
            TariffId = tariffId,
            Name = TestData.ExistingTariffs.Tariff1Name,
            PricePerMinute = TestData.ExistingTariffs.Tariff1PricePerMinute,
            BillingType = TestData.ExistingTariffs.Tariff1BillingType
        };

        TariffRepositoryMock.Setup(r => r.GetWithThemeByIdAsync(tariffId, It.IsAny<CancellationToken>())).ReturnsAsync(tariff);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Name.Should().Be(TestData.ExistingTariffs.Tariff1Name);
    }

    [Fact]
    public async Task Handler_Should_ReturnNotFound_WhenTariffDoesNotExist()
    {
        var tariffId = TestData.NonExistingIds.NonExistingTariffId;
        var query = new GetTariffByIdQuery(tariffId);

        TariffRepositoryMock.Setup(r => r.GetWithThemeByIdAsync(tariffId, It.IsAny<CancellationToken>())).ReturnsAsync((TariffWithThemeDto?)null);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task Handler_Should_ReturnFailed_WhenExceptionThrown()
    {
        var tariffId = Guid.NewGuid();
        var query = new GetTariffByIdQuery(tariffId);

        TariffRepositoryMock.Setup(r => r.GetWithThemeByIdAsync(tariffId, It.IsAny<CancellationToken>())).ThrowsAsync(new Exception());

        var result = await _handler.Handle(query, CancellationToken.None);
        result.IsFailed.Should().BeTrue();
    }
}

