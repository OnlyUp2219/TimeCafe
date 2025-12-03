namespace Venue.TimeCafe.Test.Unit.CQRS.PromotionsCqrs.Commands;

public class DeactivatePromotionCommandTests : BaseCqrsHandlerTest
{
    private readonly DeactivatePromotionCommandHandler _handler;

    public DeactivatePromotionCommandTests()
    {
        _handler = new DeactivatePromotionCommandHandler(PromotionRepositoryMock.Object);
    }

    [Fact]
    public async Task Handler_Should_ReturnSuccess_WhenPromotionDeactivated()
    {
        var command = new DeactivatePromotionCommand(1);
        var promotion = new Promotion { PromotionId = 1, Name = "Test", Description = "Desc", IsActive = true, ValidFrom = DateTime.UtcNow, ValidTo = DateTime.UtcNow.AddDays(30) };

        PromotionRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(promotion);
        PromotionRepositoryMock.Setup(r => r.DeactivateAsync(1)).ReturnsAsync(true);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Code.Should().BeNull();
    }

    [Fact]
    public async Task Handler_Should_ReturnNotFound_WhenPromotionDoesNotExist()
    {
        var command = new DeactivatePromotionCommand(999);

        PromotionRepositoryMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Promotion?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be("PromotionNotFound");
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task Handler_Should_ReturnFailed_WhenRepositoryReturnsFalse()
    {
        var command = new DeactivatePromotionCommand(1);
        var promotion = new Promotion { PromotionId = 1, Name = "Test", Description = "Desc", IsActive = true, ValidFrom = DateTime.UtcNow, ValidTo = DateTime.UtcNow.AddDays(30) };

        PromotionRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(promotion);
        PromotionRepositoryMock.Setup(r => r.DeactivateAsync(1)).ReturnsAsync(false);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be("DeactivatePromotionFailed");
        result.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task Handler_Should_ReturnFailed_WhenExceptionThrown()
    {
        var command = new DeactivatePromotionCommand(1);

        PromotionRepositoryMock.Setup(r => r.GetByIdAsync(1)).ThrowsAsync(new Exception());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be("DeactivatePromotionFailed");
        result.StatusCode.Should().Be(500);
    }

    [Theory]
    [InlineData(0, false, "ID акции обязателен")]
    [InlineData(-1, false, "ID акции обязателен")]
    [InlineData(1, true, null)]
    [InlineData(999, true, null)]
    public async Task Validator_Should_ValidateCorrectly(int promotionId, bool isValid, string? expectedError)
    {
        var command = new DeactivatePromotionCommand(promotionId);
        var validator = new DeactivatePromotionCommandValidator();

        var result = await validator.ValidateAsync(command);

        result.IsValid.Should().Be(isValid);
        if (!isValid)
        {
            result.Errors.Should().Contain(e => e.ErrorMessage.Contains(expectedError!));
        }
    }
}
