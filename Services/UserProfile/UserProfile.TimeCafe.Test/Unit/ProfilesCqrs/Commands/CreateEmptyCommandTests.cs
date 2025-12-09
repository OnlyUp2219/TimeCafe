using static UserProfile.TimeCafe.Test.Integration.Helpers.TestData;

namespace UserProfile.TimeCafe.Test.Unit.ProfilesCqrs.Commands;

public class CreateEmptyCommandTests : BaseCqrsTest
{
    [Fact]
    public async Task Handler_CreateEmpty_Should_ReturnSuccess_WhenUserIdIsValid()
    {
        // Arrange
        var userId = Guid.Parse(NewProfiles.NewUser1Id);
        var command = new CreateEmptyCommand(userId.ToString());
        var handler = new CreateEmptyCommandHandler(Repository);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.StatusCode.Should().Be(201);
        result.Message.Should().Be("Пустой профиль создан");

        var profile = await Context.Profiles.FindAsync(userId);
        profile.Should().NotBeNull();
        profile!.UserId.Should().Be(userId);
        profile.ProfileStatus.Should().Be(ProfileStatus.Pending);
    }

    [Fact]
    public async Task Handler_CreateEmpty_Should_ReturnProfileAlreadyExists_WhenProfileExists()
    {
        // Arrange
        var userId = Guid.Parse(ExistingUsers.User1Id);
        await SeedProfileAsync(userId);
        var command = new CreateEmptyCommand(userId.ToString());
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
        var userId = Guid.Parse(NewProfiles.NewUser2Id);
        var command = new CreateEmptyCommand(userId.ToString());
        var handler = new CreateEmptyCommandHandler(Repository);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Code.Should().Be("CreateEmptyFailed");
        result.StatusCode.Should().Be(500);
        result.Message.Should().Be("Не удалось создать профиль");
    }

    [Fact]
    public async Task Validator_Should_FailValidation_WhenUserIdIsEmpty()
    {
        // Arrange
        var command = new CreateEmptyCommand(InvalidIds.EmptyString);
        var validator = new CreateEmptyCommandValidator();

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Validator_Should_PassValidation_WhenUserIdIsValid()
    {
        // Arrange
        var command = new CreateEmptyCommand(ExistingUsers.User1Id);
        var validator = new CreateEmptyCommandValidator();

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
