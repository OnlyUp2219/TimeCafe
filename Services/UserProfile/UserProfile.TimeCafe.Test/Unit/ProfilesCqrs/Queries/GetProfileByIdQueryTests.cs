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
        var query = new GetProfileByIdQuery(userId);
        var handler = new GetProfileByIdQueryHandler(Repository);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.UserId.Should().Be(userId);
        result.Value.FirstName.Should().Be(ExistingUsers.User1FirstName);
        result.Value.LastName.Should().Be(ExistingUsers.User1LastName);
        result.Value.PhotoUrl.Should().Be($"/userprofile/S3/image/{userId}");
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
        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task Handler_GetProfileById_Should_ReturnFailed_WhenExceptionOccurs()
    {
        // Arrange
        await Context.DisposeAsync();
        var userId = Guid.NewGuid();
        var query = new GetProfileByIdQuery(userId);
        var handler = new GetProfileByIdQueryHandler(Repository);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();

    }

    [Fact]
    public async Task Validator_Should_FailValidation_WhenUserIdIsEmpty()
    {
        // Arrange
        var query = new GetProfileByIdQuery(Guid.Empty);
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
        var query = new GetProfileByIdQuery(Guid.Parse(ExistingUsers.User1Id));
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


