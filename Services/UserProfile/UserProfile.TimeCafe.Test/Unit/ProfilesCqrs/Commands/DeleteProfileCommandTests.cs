using static UserProfile.TimeCafe.Test.Integration.Helpers.TestData;

namespace UserProfile.TimeCafe.Test.Unit.ProfilesCqrs.Commands;

public class DeleteProfileCommandTests : BaseCqrsTest
{
    [Fact]
    public async Task Handler_DeleteProfile_Should_ReturnSuccess_WhenProfileExists()
    {
        // Arrange
        var userId = Guid.Parse(ExistingUsers.User1Id);
        await SeedProfileAsync(userId);
        var command = new DeleteProfileCommand(userId.ToString());
        var handler = new DeleteProfileCommandHandler(Repository);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Профиль успешно удалён");

        var profile = await Context.Profiles.FindAsync(userId);
        profile.Should().BeNull();
    }

    [Fact]
    public async Task Handler_DeleteProfile_Should_ReturnProfileNotFound_WhenProfileDoesNotExist()
    {
        // Arrange
        var userId = Guid.Parse(NonExistingUsers.UserId1);
        var command = new DeleteProfileCommand(userId.ToString());
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
        var userId = Guid.Parse(ExistingUsers.User2Id);
        await SeedProfileAsync(userId);
        await Context.DisposeAsync();

        var command = new DeleteProfileCommand(userId.ToString());
        var handler = new DeleteProfileCommandHandler(Repository);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Code.Should().Be("DeleteProfileFailed");
        result.StatusCode.Should().Be(500);
        result.Message.Should().Be("Не удалось удалить профиль");
    }

    [Fact]
    public async Task Validator_Should_FailValidation_WhenUserIdIsEmpty()
    {
        // Arrange
        var command = new DeleteProfileCommand(InvalidIds.EmptyString);
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
        var command = new DeleteProfileCommand(ExistingUsers.User1Id);
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
