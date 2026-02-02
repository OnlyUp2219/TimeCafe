namespace UserProfile.TimeCafe.Test.Unit.ProfilesCqrs.Commands;

public class CreateProfileCommandTests : BaseCqrsTest
{
    [Fact]
    public async Task Handler_CreateProfile_Should_ReturnSuccess_WhenValidData()
    {
        // Arrange
        var userId = Guid.Parse(NewProfiles.NewUser1Id);
        var command = new CreateProfileCommand(userId.ToString(), ExistingUsers.User1FirstName, ExistingUsers.User1LastName, ExistingUsers.User1Gender);
        var handler = new CreateProfileCommandHandler(Repository);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.StatusCode.Should().Be(201);
        result.Message.Should().Be("Профиль успешно создан");
        result.Profile.Should().NotBeNull();
        result.Profile!.UserId.Should().Be(userId);
        result.Profile.FirstName.Should().Be(ExistingUsers.User1FirstName);
        result.Profile.LastName.Should().Be(ExistingUsers.User1LastName);
        result.Profile.Gender.Should().Be(ExistingUsers.User1Gender);
    }

    [Fact]
    public async Task Handler_CreateProfile_Should_ReturnProfileAlreadyExists_WhenProfileExists()
    {
        // Arrange
        var userId = Guid.Parse(ExistingUsers.User1Id);
        await SeedProfileAsync(userId);
        var command = new CreateProfileCommand(userId.ToString(), ExistingUsers.User1FirstName, ExistingUsers.User1LastName, ExistingUsers.User1Gender);
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
    public async Task Handler_CreateProfile_Should_ThrowCqrsResultException_WhenExceptionOccurs()
    {
        // Arrange
        await Context.DisposeAsync();
        var userId = Guid.Parse(NewProfiles.NewUser2Id);
        var command = new CreateProfileCommand(userId.ToString(), ExistingUsers.User1FirstName, ExistingUsers.User1LastName, ExistingUsers.User1Gender);
        var handler = new CreateProfileCommandHandler(Repository);

        // Act
        var ex = await Assert.ThrowsAsync<CqrsResultException>(
            () => handler.Handle(command, CancellationToken.None));

        // Assert
        ex.Result.Should().NotBeNull();
        ex.Result!.Success.Should().BeFalse();
        ex.Result.Code.Should().Be("CreateProfileFailed");
        ex.Result.StatusCode.Should().Be(500);
        ex.Result.Message.Should().Be("Не удалось создать профиль");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Validator_Should_FailValidation_WhenFirstNameEmpty(string? firstName)
    {
        // Arrange
        var userId = Guid.Parse(NonExistingUsers.UserId1);
        var command = new CreateProfileCommand(userId.ToString(), firstName ?? "", ExistingUsers.User1LastName, ExistingUsers.User1Gender);
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
        var userId = Guid.Parse(NonExistingUsers.UserId2);
        var command = new CreateProfileCommand(
            userId.ToString(),
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
        var userId = Guid.Parse(NewProfiles.NewUser1Id);
        var command = new CreateProfileCommand(userId.ToString(), ExistingUsers.User1FirstName, ExistingUsers.User1LastName, Gender.Female);
        var handler = new CreateProfileCommandHandler(Repository);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Profile!.ProfileStatus.Should().Be(ProfileStatus.Pending);
    }
}
