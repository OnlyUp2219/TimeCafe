namespace UserProfile.TimeCafe.Test.Unit.ProfilesCqrs.Commands;

public class UpdateProfileCommandTests : BaseCqrsTest
{
    [Fact]
    public async Task Handler_UpdateProfile_Should_ReturnSuccess_WhenProfileExists()
    {
        // Arrange
        await SeedProfileAsync("user123", "Иван", "Петров");
        var updatedProfile = new Profile
        {
            UserId = "user123",
            FirstName = "Александр",
            LastName = "Сидоров",
            Gender = Gender.Male,
            ProfileStatus = ProfileStatus.Completed
        };
        var command = new UpdateProfileCommand(updatedProfile);
        var handler = new UpdateProfileCommandHandler(Repository);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Профиль успешно обновлён");
        result.Profile.Should().NotBeNull();
        result.Profile!.FirstName.Should().Be("Александр");
        result.Profile.LastName.Should().Be("Сидоров");
    }

    [Fact]
    public async Task Handler_UpdateProfile_Should_ReturnProfileNotFound_WhenProfileDoesNotExist()
    {
        // Arrange
        var profile = new Profile
        {
            UserId = "nonexistent",
            FirstName = "Test",
            LastName = "User"
        };
        var command = new UpdateProfileCommand(profile);
        var handler = new UpdateProfileCommandHandler(Repository);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Code.Should().Be("ProfileNotFound");
        result.StatusCode.Should().Be(404);
        result.Message.Should().Be("Профиль не найден");
    }

    [Fact]
    public async Task Handler_UpdateProfile_Should_ReturnUpdateFailed_WhenExceptionOccurs()
    {
        // Arrange
        await SeedProfileAsync("user123");
        await Context.DisposeAsync();

        var profile = new Profile { UserId = "user123", FirstName = "Test", LastName = "User" };
        var command = new UpdateProfileCommand(profile);
        var handler = new UpdateProfileCommandHandler(Repository);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Code.Should().Be("UpdateProfileFailed");
        result.StatusCode.Should().Be(500);
        result.Message.Should().Be("Не удалось обновить профиль");
    }

    [Fact]
    public async Task Validator_Should_FailValidation_WhenProfileIsNull()
    {
        // Arrange
        var command = new UpdateProfileCommand(null!);
        var validator = new UpdateProfileCommandValidator();

        // Act & Assert
        await Assert.ThrowsAsync<NullReferenceException>(async () =>
            await validator.ValidateAsync(command));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Validator_Should_FailValidation_WhenUserIdIsEmpty(string? userId)
    {
        // Arrange
        var profile = new Profile { UserId = userId!, FirstName = "Test", LastName = "User" };
        var command = new UpdateProfileCommand(profile);
        var validator = new UpdateProfileCommandValidator();

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Validator_Should_FailValidation_WhenFieldsExceedMaxLength()
    {
        // Arrange
        var profile = new Profile
        {
            UserId = new string('a', 451),
            FirstName = new string('b', 101),
            LastName = new string('c', 101)
        };
        var command = new UpdateProfileCommand(profile);
        var validator = new UpdateProfileCommandValidator();

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Count.Should().BeGreaterThanOrEqualTo(3);
    }
}
