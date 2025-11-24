namespace UserProfile.TimeCafe.Test.Unit.ProfilesCqrs.Commands;

public class CreateEmptyCommandTests : BaseCqrsTest
{
    [Fact]
    public async Task Handler_CreateEmpty_Should_ReturnSuccess_WhenUserIdIsValid()
    {
        // Arrange
        var command = new CreateEmptyCommand("user123");
        var handler = new CreateEmptyCommandHandler(Repository);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.StatusCode.Should().Be(201);
        result.Message.Should().Be("Пустой профиль создан");

        var profile = await Context.Profiles.FindAsync("user123");
        profile.Should().NotBeNull();
        profile!.UserId.Should().Be("user123");
        profile.ProfileStatus.Should().Be(ProfileStatus.Pending);
    }

    [Fact]
    public async Task Handler_CreateEmpty_Should_ReturnProfileAlreadyExists_WhenProfileExists()
    {
        // Arrange
        await SeedProfileAsync("user123");
        var command = new CreateEmptyCommand("user123");
        var handler = new CreateEmptyCommandHandler(Repository);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Code.Should().Be("ProfileAlreadyExists");
        result.StatusCode.Should().Be(409);
        result.Message.Should().Be("Профиль уже существует");
    }

    [Fact]
    public async Task Handler_CreateEmpty_Should_ReturnCreateFailed_WhenExceptionOccurs()
    {
        // Arrange
        await Context.DisposeAsync();
        var command = new CreateEmptyCommand("user123");
        var handler = new CreateEmptyCommandHandler(Repository);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Code.Should().Be("CreateEmptyFailed");
        result.StatusCode.Should().Be(500);
        result.Message.Should().Be("Не удалось создать профиль");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Validator_Should_FailValidation_WhenUserIdIsEmpty(string? userId)
    {
        // Arrange
        var command = new CreateEmptyCommand(userId!);
        var validator = new CreateEmptyCommandValidator();

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
        var command = new CreateEmptyCommand(new string('a', 65));
        var validator = new CreateEmptyCommandValidator();

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "UserId");
    }
}
