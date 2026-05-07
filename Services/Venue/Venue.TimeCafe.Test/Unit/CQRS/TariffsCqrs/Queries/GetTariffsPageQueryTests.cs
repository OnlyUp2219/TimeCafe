using Microsoft.Extensions.Options;
using Venue.TimeCafe.Application.Options;

namespace Venue.TimeCafe.Test.Unit.CQRS.TariffsCqrs.Queries;

public class GetTariffsPageQueryTests : BaseCqrsHandlerTest
{
    private readonly GetTariffsPageQueryHandler _handler;
    private readonly Mock<IOptionsSnapshot<VenuePricingOptions>> _optionsMock;

    public GetTariffsPageQueryTests()
    {
        _optionsMock = new Mock<IOptionsSnapshot<VenuePricingOptions>>();
        _optionsMock.Setup(o => o.Value).Returns(new VenuePricingOptions { MaxTotalDiscountPercent = 20 });
        _handler = new GetTariffsPageQueryHandler(UowMock.Object, _optionsMock.Object);
    }

    [Fact]
    public async Task Handler_Should_ReturnSuccess_WhenPageFound()
    {
        var query = new GetTariffsPageQuery(TestData.DefaultValues.FirstPage, TestData.DefaultValues.DefaultPageSize);
        var tariffs = new List<TariffWithThemeDto>
        {
            new() { TariffId = Guid.NewGuid(), Name = TestData.ExistingTariffs.Tariff1Name, PricePerMinute = TestData.ExistingTariffs.Tariff1PricePerMinute },
            new() { TariffId = Guid.NewGuid(), Name = TestData.ExistingTariffs.Tariff2Name, PricePerMinute = TestData.ExistingTariffs.Tariff2PricePerMinute }
        };

        TariffRepositoryMock.Setup(r => r.GetPagedAsync(TestData.DefaultValues.FirstPage, TestData.DefaultValues.DefaultPageSize, It.IsAny<CancellationToken>())).ReturnsAsync(tariffs);
        TariffRepositoryMock.Setup(r => r.GetTotalCountAsync(It.IsAny<CancellationToken>())).ReturnsAsync(15);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Tariffs.Should().HaveCount(2);
        result.Value.TotalCount.Should().Be(15);
    }

    [Fact]
    public async Task Handler_Should_ReturnSuccess_WhenNoTariffsOnPage()
    {
        var query = new GetTariffsPageQuery(10, TestData.DefaultValues.DefaultPageSize);
        var tariffs = new List<TariffWithThemeDto>();

        TariffRepositoryMock.Setup(r => r.GetPagedAsync(10, TestData.DefaultValues.DefaultPageSize, It.IsAny<CancellationToken>())).ReturnsAsync(tariffs);
        TariffRepositoryMock.Setup(r => r.GetTotalCountAsync(It.IsAny<CancellationToken>())).ReturnsAsync(5);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Tariffs.Should().BeEmpty();
        result.Value.TotalCount.Should().Be(5);
    }

    [Fact]
    public async Task Handler_Should_ReturnFailed_WhenExceptionThrown()
    {
        var query = new GetTariffsPageQuery(TestData.DefaultValues.FirstPage, TestData.DefaultValues.DefaultPageSize);

        TariffRepositoryMock.Setup(r => r.GetPagedAsync(TestData.DefaultValues.FirstPage, TestData.DefaultValues.DefaultPageSize, It.IsAny<CancellationToken>())).ThrowsAsync(new Exception());

        var result = await _handler.Handle(query, CancellationToken.None);
        result.IsFailed.Should().BeTrue();
    }
}

