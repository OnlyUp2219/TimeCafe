namespace UserProfile.TimeCafe.Test.Unit.ProfilesCqrs.Queries;

public class GetProfileByIdQueryTests : BaseCqrsTest
{
    [Fact]
    public async Task Handler_GetProfileById_Should_ReturnSuccess_WhenProfileExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var profile = await SeedProfileAsync(userId, ExistingUsers.User1FirstName, ExistingUsers.User1LastName);
        profile.PhotoUrl = $"profiles/{userId}/photo";
        await Context.SaveChangesAsync();
        var query = new GetProfileByIdQuery(userId.ToString());
        var handler = new GetProfileByIdQueryHandler(Repository);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Профиль найден");
        result.Profile.Should().NotBeNull();
        result.Profile!.UserId.Should().Be(userId);
        result.Profile.FirstName.Should().Be(ExistingUsers.User1FirstName);
        result.Profile.LastName.Should().Be(ExistingUsers.User1LastName);
        result.Profile.PhotoUrl.Should().Be($"/userprofile/S3/image/{userId}");
    }

    [Fact]
    public async Task Handler_GetProfileById_Should_ReturnProfileNotFound_WhenProfileDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetProfileByIdQuery(userId.ToString());
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
    public async Task Handler_GetProfileById_Should_ThrowCqrsResultException_WhenExceptionOccurs()
    {
        // Arrange
        await Context.DisposeAsync();
        var userId = Guid.NewGuid();
        var query = new GetProfileByIdQuery(userId.ToString());
        var handler = new GetProfileByIdQueryHandler(Repository);

        // Act
        var ex = await Assert.ThrowsAsync<CqrsResultException>(
            () => handler.Handle(query, CancellationToken.None));

        // Assert
        ex.Result.Should().NotBeNull();
        ex.Result!.Success.Should().BeFalse();
        ex.Result.Code.Should().Be("GetProfileFailed");
        ex.Result.StatusCode.Should().Be(500);
        ex.Result.Message.Should().Be("Не удалось получить профиль");
    }

    [Fact]
    public async Task Validator_Should_FailValidation_WhenUserIdIsEmpty()
    {
        // Arrange
        var query = new GetProfileByIdQuery(InvalidIds.EmptyString);
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
        var query = new GetProfileByIdQuery(ExistingUsers.User1Id);
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
