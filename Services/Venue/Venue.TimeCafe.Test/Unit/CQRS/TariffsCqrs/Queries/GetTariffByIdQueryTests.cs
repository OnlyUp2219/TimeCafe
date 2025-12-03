namespace Venue.TimeCafe.Test.Unit.CQRS.TariffsCqrs.Queries;

public class GetTariffByIdQueryTests : BaseCqrsHandlerTest
{
    private readonly GetTariffByIdQueryHandler _handler;

    public GetTariffByIdQueryTests()
    {
        _handler = new GetTariffByIdQueryHandler(TariffRepositoryMock.Object);
    }

    [Fact]
    public async Task Handler_Should_ReturnSuccess_WhenTariffFound()
    {
        var query = new GetTariffByIdQuery(1);
        var tariff = new Tariff { TariffId = 1, Name = "Test Tariff", PricePerMinute = 10m, BillingType = BillingType.PerMinute };

        TariffRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(tariff);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Tariff.Should().NotBeNull();
        result.Tariff!.Name.Should().Be("Test Tariff");
    }

    [Fact]
    public async Task Handler_Should_ReturnNotFound_WhenTariffDoesNotExist()
    {
        var query = new GetTariffByIdQuery(999);

        TariffRepositoryMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Tariff?)null);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be("TariffNotFound");
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task Handler_Should_ReturnFailed_WhenExceptionThrown()
    {
        var query = new GetTariffByIdQuery(1);

        TariffRepositoryMock.Setup(r => r.GetByIdAsync(1)).ThrowsAsync(new Exception());

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be("GetTariffFailed");
        result.StatusCode.Should().Be(500);
    }

    [Theory]
    [InlineData(0, false, "ID тарифа обязателен")]
    [InlineData(-1, false, "ID тарифа обязателен")]
    [InlineData(1, true, null)]
    [InlineData(999, true, null)]
    public async Task Validator_Should_ValidateCorrectly(int tariffId, bool isValid, string? expectedError)
    {
        var query = new GetTariffByIdQuery(tariffId);
        var validator = new GetTariffByIdQueryValidator();

        var result = await validator.ValidateAsync(query);

        result.IsValid.Should().Be(isValid);
        if (!isValid)
        {
            result.Errors.Should().Contain(e => e.ErrorMessage.Contains(expectedError!));
        }
    }
}
