namespace Venue.TimeCafe.Test.Unit.CQRS.TariffsCqrs.Queries;

public class GetTariffsPageQueryTests : BaseCqrsHandlerTest
{
    private readonly GetTariffsPageQueryHandler _handler;

    public GetTariffsPageQueryTests()
    {
        _handler = new GetTariffsPageQueryHandler(TariffRepositoryMock.Object);
    }

    [Fact]
    public async Task Handler_Should_ReturnSuccess_WhenPageFound()
    {
        var query = new GetTariffsPageQuery(1, 10);
        var tariffs = new List<Tariff>
        {
            new() { TariffId = 1, Name = "Tariff 1", PricePerMinute = 10m },
            new() { TariffId = 2, Name = "Tariff 2", PricePerMinute = 20m }
        };

        TariffRepositoryMock.Setup(r => r.GetPagedAsync(1, 10)).ReturnsAsync(tariffs);
        TariffRepositoryMock.Setup(r => r.GetTotalCountAsync()).ReturnsAsync(15);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Tariffs.Should().NotBeNull();
        result.Tariffs.Should().HaveCount(2);
        result.TotalCount.Should().Be(15);
    }

    [Fact]
    public async Task Handler_Should_ReturnSuccess_WhenNoTariffsOnPage()
    {
        var query = new GetTariffsPageQuery(10, 10);
        var tariffs = new List<Tariff>();

        TariffRepositoryMock.Setup(r => r.GetPagedAsync(10, 10)).ReturnsAsync(tariffs);
        TariffRepositoryMock.Setup(r => r.GetTotalCountAsync()).ReturnsAsync(5);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Tariffs.Should().NotBeNull();
        result.Tariffs.Should().BeEmpty();
        result.TotalCount.Should().Be(5);
    }

    [Fact]
    public async Task Handler_Should_ReturnFailed_WhenExceptionThrown()
    {
        var query = new GetTariffsPageQuery(1, 10);

        TariffRepositoryMock.Setup(r => r.GetPagedAsync(1, 10)).ThrowsAsync(new Exception());

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be("GetTariffsPageFailed");
        result.StatusCode.Should().Be(500);
    }

    [Theory]
    [InlineData(0, 10, false, "Номер страницы должен быть больше 0")]
    [InlineData(-1, 10, false, "Номер страницы должен быть больше 0")]
    [InlineData(1, 0, false, "Размер страницы должен быть больше 0")]
    [InlineData(1, -1, false, "Размер страницы должен быть больше 0")]
    [InlineData(1, 101, false, "Размер страницы не может превышать 100")]
    [InlineData(1, 10, true, null)]
    [InlineData(1, 100, true, null)]
    public async Task Validator_Should_ValidateCorrectly(int pageNumber, int pageSize, bool isValid, string? expectedError)
    {
        var query = new GetTariffsPageQuery(pageNumber, pageSize);
        var validator = new GetTariffsPageQueryValidator();

        var result = await validator.ValidateAsync(query);

        result.IsValid.Should().Be(isValid);
        if (!isValid)
        {
            result.Errors.Should().Contain(e => e.ErrorMessage.Contains(expectedError!));
        }
    }
}
