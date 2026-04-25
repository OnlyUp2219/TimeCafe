namespace UserProfile.TimeCafe.Test.Unit.ProfilesCqrs.Commands;

public class CreateProfileCommandTests : BaseCqrsTest
{
    [Fact]
    public async Task Handler_CreateProfile_Should_ReturnSuccess_WhenValidData()
    {
        // Arrange
        var userId = Guid.Parse(NewProfiles.NewUser1Id);
        var command = new CreateProfileCommand(userId, ExistingUsers.User1FirstName, ExistingUsers.User1LastName, ExistingUsers.User1Gender);
        var handler = new CreateProfileCommandHandler(Repository);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.UserId.Should().Be(userId);
        result.Value.FirstName.Should().Be(ExistingUsers.User1FirstName);
        result.Value.LastName.Should().Be(ExistingUsers.User1LastName);
        result.Value.Gender.Should().Be(ExistingUsers.User1Gender);
    }

    [Fact]
    public async Task Handler_CreateProfile_Should_ReturnProfileAlreadyExists_WhenProfileExists()
    {
        // Arrange
        var userId = Guid.Parse(ExistingUsers.User1Id);
        await SeedProfileAsync(userId);
        var command = new CreateProfileCommand(userId, ExistingUsers.User1FirstName, ExistingUsers.User1LastName, ExistingUsers.User1Gender);
        var handler = new CreateProfileCommandHandler(Repository);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task Handler_CreateProfile_Should_ReturnFailed_WhenExceptionOccurs()
    {
        // Arrange
        await Context.DisposeAsync();
        var userId = Guid.Parse(NewProfiles.NewUser2Id);
        var command = new CreateProfileCommand(userId, ExistingUsers.User1FirstName, ExistingUsers.User1LastName, ExistingUsers.User1Gender);
        var handler = new CreateProfileCommandHandler(Repository);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();

    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Validator_Should_FailValidation_WhenFirstNameEmpty(string? firstName)
    {
        // Arrange
        var userId = Guid.Parse(NonExistingUsers.UserId1);
        var command = new CreateProfileCommand(userId, firstName ?? "", ExistingUsers.User1LastName, ExistingUsers.User1Gender);
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
        var userId = Guid.Parse(NewProfiles.NewUser1Id);
        var command = new CreateProfileCommand(userId, ExistingUsers.User1FirstName, ExistingUsers.User1LastName, Gender.Female);
        var handler = new CreateProfileCommandHandler(Repository);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.ProfileStatus.Should().Be(ProfileStatus.Pending);
    }
}


