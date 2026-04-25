namespace UserProfile.TimeCafe.Test.Unit.ProfilesCqrs.Commands;

public class DeleteProfileCommandTests : BaseCqrsTest
{
    [Fact]
    public async Task Handler_DeleteProfile_Should_ReturnSuccess_WhenProfileExists()
    {
        // Arrange
        var userId = Guid.Parse(ExistingUsers.User1Id);
        await SeedProfileAsync(userId);
        var command = new DeleteProfileCommand(userId);
        var handler = new DeleteProfileCommandHandler(Repository);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var profile = await Context.Profiles.FindAsync(userId);
        profile.Should().BeNull();
    }

    [Fact]
    public async Task Handler_DeleteProfile_Should_ReturnProfileNotFound_WhenProfileDoesNotExist()
    {
        // Arrange
        var userId = Guid.Parse(NonExistingUsers.UserId1);
        var command = new DeleteProfileCommand(userId);
        var handler = new DeleteProfileCommandHandler(Repository);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task Handler_DeleteProfile_Should_ReturnFailed_WhenExceptionOccurs()
    {
        // Arrange
        var userId = Guid.Parse(ExistingUsers.User2Id);
        await SeedProfileAsync(userId);
        await Context.DisposeAsync();

        var command = new DeleteProfileCommand(userId);
        var handler = new DeleteProfileCommandHandler(Repository);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();

    }

    [Fact]
    public async Task Validator_Should_FailValidation_WhenUserIdIsEmpty()
    {
        // Arrange
        var command = new DeleteProfileCommand(Guid.Empty);
        var validator = new DeleteProfileCommandValidator();

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Validator_Should_PassValidation_WhenUserIdIsValid()
    {
        // Arrange
        var command = new DeleteProfileCommand(Guid.Parse(ExistingUsers.User1Id));
        var validator = new DeleteProfileCommandValidator();

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validator_Should_FailValidation_WhenUserIdIsTooLong()
    {
        // This test is no longer applicable as UserId is now Guid (fixed size)
        // Keeping this as a placeholder showing the change
    }
}


