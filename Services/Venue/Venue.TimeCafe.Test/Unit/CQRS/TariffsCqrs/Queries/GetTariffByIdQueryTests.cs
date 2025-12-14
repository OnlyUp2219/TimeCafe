using Venue.TimeCafe.Domain.DTOs;
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
        var query = new GetTariffByIdQuery(TestData.ExistingThemes.Theme1Id.ToString());
        var tariff = new TariffWithThemeDto { TariffId = TestData.ExistingThemes.Theme1Id, TariffName = "Test Tariff", TariffPricePerMinute = 10m, TariffBillingType = BillingType.PerMinute };

        TariffRepositoryMock.Setup(r => r.GetByIdAsync(It.Is<Guid>(id => id == TestData.ExistingThemes.Theme1Id))).ReturnsAsync(tariff);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Tariff.Should().NotBeNull();
        result.Tariff!.TariffName.Should().Be("Test Tariff");
    }

    [Fact]
    public async Task Handler_Should_ReturnNotFound_WhenTariffDoesNotExist()
    {
        var query = new GetTariffByIdQuery(TestData.NonExistingIds.NonExistingTariffId.ToString());

        TariffRepositoryMock.Setup(r => r.GetByIdAsync(It.Is<Guid>(id => id == TestData.NonExistingIds.NonExistingTariffId))).ReturnsAsync((TariffWithThemeDto?)null);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be("TariffNotFound");
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task Handler_Should_ReturnFailed_WhenExceptionThrown()
    {
        var query = new GetTariffByIdQuery(TestData.ExistingThemes.Theme1Id.ToString());

        TariffRepositoryMock.Setup(r => r.GetByIdAsync(It.Is<Guid>(id => id == TestData.ExistingThemes.Theme1Id))).ThrowsAsync(new Exception());

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be("GetTariffFailed");
        result.StatusCode.Should().Be(500);
    }

    [Theory]
    [InlineData("", false, "Тариф не найден")]
    [InlineData("not-a-guid", false, "Тариф не найден")]
    [InlineData("00000000-0000-0000-0000-000000000000", false, "Тариф не найден")]
    [InlineData("a1111111-1111-1111-1111-111111111111", true, null)]
    [InlineData("99999999-9999-9999-9999-999999999999", true, null)]
    public async Task Validator_Should_ValidateCorrectly(string tariffId, bool isValid, string? expectedError)
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
