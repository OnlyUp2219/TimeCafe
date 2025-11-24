namespace UserProfile.TimeCafe.Test.Unit.ProfilesCqrs.Commands;

public class CreateProfileCommandTests : BaseCqrsTest
{
    [Fact]
    public async Task Handler_CreateProfile_Should_ReturnSuccess_WhenValidData()
    {
        // Arrange
        var command = new CreateProfileCommand("user123", "Иван", "Петров", Gender.Male);
        var handler = new CreateProfileCommandHandler(Repository);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.StatusCode.Should().Be(201);
        result.Message.Should().Be("Профиль успешно создан");
        result.Profile.Should().NotBeNull();
        result.Profile!.UserId.Should().Be("user123");
        result.Profile.FirstName.Should().Be("Иван");
        result.Profile.LastName.Should().Be("Петров");
        result.Profile.Gender.Should().Be(Gender.Male);
    }

    [Fact]
    public async Task Handler_CreateProfile_Should_ReturnProfileAlreadyExists_WhenProfileExists()
    {
        // Arrange
        await SeedProfileAsync("user123");
        var command = new CreateProfileCommand("user123", "Иван", "Петров", Gender.Male);
        var handler = new CreateProfileCommandHandler(Repository);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Code.Should().Be("ProfileAlreadyExists");
        result.StatusCode.Should().Be(409);
        result.Message.Should().Be("Профиль для пользователя уже существует");
    }

    [Fact]
    public async Task Handler_CreateProfile_Should_ReturnCreateFailed_WhenExceptionOccurs()
    {
        // Arrange
        await Context.DisposeAsync();
        var command = new CreateProfileCommand("user123", "Иван", "Петров", Gender.Male);
        var handler = new CreateProfileCommandHandler(Repository);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Code.Should().Be("CreateProfileFailed");
        result.StatusCode.Should().Be(500);
        result.Message.Should().Be("Не удалось создать профиль");
    }

    [Theory]
    [InlineData("", "Иван", "Петров")]
    [InlineData("user123", "", "Петров")]
    [InlineData("user123", "Иван", "")]
    public async Task Validator_Should_FailValidation_WhenRequiredFieldsEmpty(string userId, string firstName, string lastName)
    {
        // Arrange
        var command = new CreateProfileCommand(userId, firstName, lastName, Gender.Male);
        var validator = new CreateProfileCommandValidator();

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Validator_Should_FailValidation_WhenFieldsExceedMaxLength()
    {
        // Arrange
        var command = new CreateProfileCommand(
            new string('a', 65),
            new string('b', 129),
            new string('c', 129),
            Gender.Male);
        var validator = new CreateProfileCommandValidator();

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Count.Should().BeGreaterThanOrEqualTo(3);
    }

    [Fact]
    public async Task Handler_CreateProfile_Should_SetPendingStatus_WhenCreated()
    {
        // Arrange
        var command = new CreateProfileCommand("user123", "Иван", "Петров", Gender.Female);
        var handler = new CreateProfileCommandHandler(Repository);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Profile!.ProfileStatus.Should().Be(ProfileStatus.Pending);
    }
}
