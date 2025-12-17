using Venue.TimeCafe.Domain.DTOs;
using Venue.TimeCafe.Test.Integration.Helpers;

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
        var tariffId = Guid.NewGuid();
        var query = new GetTariffByIdQuery(tariffId.ToString());
        var tariff = new TariffWithThemeDto
        {
            TariffId = tariffId,
            TariffName = TestData.ExistingTariffs.Tariff1Name,
            TariffPricePerMinute = TestData.ExistingTariffs.Tariff1PricePerMinute,
            TariffBillingType = TestData.ExistingTariffs.Tariff1BillingType
        };

        TariffRepositoryMock.Setup(r => r.GetByIdAsync(tariffId)).ReturnsAsync(tariff);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Tariff.Should().NotBeNull();
        result.Tariff!.TariffName.Should().Be(TestData.ExistingTariffs.Tariff1Name);
    }

    [Fact]
    public async Task Handler_Should_ReturnNotFound_WhenTariffDoesNotExist()
    {
        var tariffId = TestData.NonExistingIds.NonExistingTariffId;
        var query = new GetTariffByIdQuery(tariffId.ToString());

        TariffRepositoryMock.Setup(r => r.GetByIdAsync(tariffId)).ReturnsAsync((TariffWithThemeDto?)null);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be("TariffNotFound");
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task Handler_Should_ReturnFailed_WhenExceptionThrown()
    {
        var tariffId = Guid.NewGuid();
        var query = new GetTariffByIdQuery(tariffId.ToString());

        TariffRepositoryMock.Setup(r => r.GetByIdAsync(tariffId)).ThrowsAsync(new Exception());

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be("GetTariffFailed");
        result.StatusCode.Should().Be(500);
    }

    [Theory]
    [InlineData("", false, "Тариф не найден")]
    [InlineData("not-a-guid", false, "Тариф не найден")]
    [InlineData("00000000-0000-0000-0000-000000000000", false, "Тариф не найден")]
    public async Task Validator_Should_ValidateCorrectly_InvalidCases(string tariffId, bool isValid, string? expectedError)
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

    [Fact]
    public async Task Validator_Should_ValidateCorrectly_ValidGuid()
    {
        var query = new GetTariffByIdQuery(Guid.NewGuid().ToString());
        var validator = new GetTariffByIdQueryValidator();

        var result = await validator.ValidateAsync(query);

        result.IsValid.Should().BeTrue();
    }
}
