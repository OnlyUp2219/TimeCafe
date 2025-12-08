namespace UserProfile.TimeCafe.Test.Unit.ProfilesCqrs.Queries;

public class GetProfileByIdQueryTests : BaseCqrsTest
{
    [Fact]
    public async Task Handler_GetProfileById_Should_ReturnSuccess_WhenProfileExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        await SeedProfileAsync(userId, "Иван", "Петров");
        var query = new GetProfileByIdQuery(userId);
        var handler = new GetProfileByIdQueryHandler(Repository);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Профиль найден");
        result.Profile.Should().NotBeNull();
        result.Profile!.UserId.Should().Be(userId);
        result.Profile.FirstName.Should().Be("Иван");
        result.Profile.LastName.Should().Be("Петров");
    }

    [Fact]
    public async Task Handler_GetProfileById_Should_ReturnProfileNotFound_WhenProfileDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetProfileByIdQuery(userId);
        var handler = new GetProfileByIdQueryHandler(Repository);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Code.Should().Be("ProfileNotFound");
        result.StatusCode.Should().Be(404);
        result.Message.Should().Be("Профиль не найден");
        result.Profile.Should().BeNull();
    }

    [Fact]
    public async Task Handler_GetProfileById_Should_ReturnGetFailed_WhenExceptionOccurs()
    {
        // Arrange
        await Context.DisposeAsync();
        var userId = Guid.NewGuid();
        var query = new GetProfileByIdQuery(userId);
        var handler = new GetProfileByIdQueryHandler(Repository);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Code.Should().Be("GetProfileFailed");
        result.StatusCode.Should().Be(500);
        result.Message.Should().Be("Не удалось получить профиль");
    }

    [Fact]
    public async Task Validator_Should_FailValidation_WhenUserIdIsEmpty()
    {
        // Arrange
        var userId = Guid.Empty; // Empty GUID represents invalid user
        var query = new GetProfileByIdQuery(userId);
        var validator = new GetProfileByIdQueryValidator();

        // Act
        var result = await validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Validator_Should_PassValidation_WhenUserIdIsValid()
    {
        // Arrange
        var query = new GetProfileByIdQuery(Guid.NewGuid());
        var validator = new GetProfileByIdQueryValidator();

        // Act
        var result = await validator.ValidateAsync(query);

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
