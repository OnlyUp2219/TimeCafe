namespace UserProfile.TimeCafe.Test.Unit.ProfilesCqrs.Commands;

public class DeleteProfileCommandTests : BaseCqrsTest
{
    [Fact]
    public async Task Handler_DeleteProfile_Should_ReturnSuccess_WhenProfileExists()
    {
        // Arrange
        await SeedProfileAsync("user123");
        var command = new DeleteProfileCommand("user123");
        var handler = new DeleteProfileCommandHandler(Repository);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Профиль успешно удалён");

        var profile = await Context.Profiles.FindAsync("user123");
        profile.Should().BeNull();
    }

    [Fact]
    public async Task Handler_DeleteProfile_Should_ReturnProfileNotFound_WhenProfileDoesNotExist()
    {
        // Arrange
        var command = new DeleteProfileCommand("nonexistent");
        var handler = new DeleteProfileCommandHandler(Repository);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Code.Should().Be("ProfileNotFound");
        result.StatusCode.Should().Be(404);
        result.Message.Should().Be("Профиль не найден");
    }

    [Fact]
    public async Task Handler_DeleteProfile_Should_ReturnDeleteFailed_WhenExceptionOccurs()
    {
        // Arrange
        await SeedProfileAsync("user123");
        await Context.DisposeAsync();

        var command = new DeleteProfileCommand("user123");
        var handler = new DeleteProfileCommandHandler(Repository);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Code.Should().Be("DeleteProfileFailed");
        result.StatusCode.Should().Be(500);
        result.Message.Should().Be("Не удалось удалить профиль");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Validator_Should_FailValidation_WhenUserIdIsEmpty(string? userId)
    {
        // Arrange
        var command = new DeleteProfileCommand(userId!);
        var validator = new DeleteProfileCommandValidator();

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "UserId");
    }

    [Fact]
    public async Task Validator_Should_FailValidation_WhenUserIdIsTooLong()
    {
        // Arrange
        var command = new DeleteProfileCommand(new string('a', 451));
        var validator = new DeleteProfileCommandValidator();

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "UserId");
    }
}
