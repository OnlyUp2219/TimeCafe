namespace UserProfile.TimeCafe.Test.Unit.ProfilesCqrs.Commands;

public class CreateProfileCommandTests : BaseCqrsTest
{
    [Fact]
    public async Task Handler_CreateProfile_Should_ReturnSuccess_WhenValidData()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new CreateProfileCommand(userId, "Иван", "Петров", Gender.Male);
        var handler = new CreateProfileCommandHandler(Repository);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.StatusCode.Should().Be(201);
        result.Message.Should().Be("Профиль успешно создан");
        result.Profile.Should().NotBeNull();
        result.Profile!.UserId.Should().Be(userId);
        result.Profile.FirstName.Should().Be("Иван");
        result.Profile.LastName.Should().Be("Петров");
        result.Profile.Gender.Should().Be(Gender.Male);
    }

    [Fact]
    public async Task Handler_CreateProfile_Should_ReturnProfileAlreadyExists_WhenProfileExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        await SeedProfileAsync(userId);
        var command = new CreateProfileCommand(userId, "Иван", "Петров", Gender.Male);
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
        var userId = Guid.NewGuid();
        var command = new CreateProfileCommand(userId, "Иван", "Петров", Gender.Male);
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
    [InlineData("")]
    [InlineData(null)]
    public async Task Validator_Should_FailValidation_WhenFirstNameEmpty(string? firstName)
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new CreateProfileCommand(userId, firstName ?? "", "Петров", Gender.Male);
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
        var userId = Guid.NewGuid();
        var command = new CreateProfileCommand(
            userId,
            new string('b', 101),
            new string('c', 101),
            Gender.Male);
        var validator = new CreateProfileCommandValidator();

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Count.Should().Be(2);
    }

    [Fact]
    public async Task Handler_CreateProfile_Should_SetPendingStatus_WhenCreated()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new CreateProfileCommand(userId, "Иван", "Петров", Gender.Female);
        var handler = new CreateProfileCommandHandler(Repository);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Profile!.ProfileStatus.Should().Be(ProfileStatus.Pending);
    }
}
