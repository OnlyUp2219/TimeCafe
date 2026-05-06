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
        var handler = new GetProfileByIdQueryHandler(Uow);

        // Act
        var result = await handler.Handle(query);

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
        var handler = new GetProfileByIdQueryHandler(Uow);

        // Act
        var result = await handler.Handle(query);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.HasError<ProfileNotFoundError>().Should().BeTrue();
    }

    [Fact]
    public async Task Handler_GetProfileById_Should_ReturnFailed_WhenExceptionOccurs()
    {
        // Arrange
        await Context.DisposeAsync();
        var userId = Guid.NewGuid();
        var query = new GetProfileByIdQuery(userId);
        var handler = new GetProfileByIdQueryHandler(Uow);

        // Act
        var result = await handler.Handle(query);

        // Assert
        result.IsFailed.Should().BeTrue();
    }
}
