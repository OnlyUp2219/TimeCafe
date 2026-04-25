namespace UserProfile.TimeCafe.Test.Unit.ProfilesCqrs.Queries;

public class GetAllProfilesQueryTests : BaseCqrsTest
{
    [Fact]
    public async Task Handler_GetAllProfiles_Should_ReturnSuccess_WhenProfilesExist()
    {
        // Arrange
        var userId1 = Guid.NewGuid();
        var userId2 = Guid.NewGuid();
        var userId3 = Guid.NewGuid();

        await SeedProfileAsync(userId1, ExistingUsers.User1FirstName, ExistingUsers.User1LastName);
        await SeedProfileAsync(userId2, ExistingUsers.User2FirstName, ExistingUsers.User2LastName);
        await SeedProfileAsync(userId3, ExistingUsers.User3FirstName, ExistingUsers.User3LastName);

        var query = new GetAllProfilesQuery();
        var handler = new GetAllProfilesQueryHandler(Repository);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Should().HaveCount(3);
    }

    [Fact]
    public async Task Handler_GetAllProfiles_Should_ReturnEmptyList_WhenNoProfiles()
    {
        // Arrange
        var query = new GetAllProfilesQuery();
        var handler = new GetAllProfilesQueryHandler(Repository);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Should().BeEmpty();
    }

    [Fact]
    public async Task Handler_GetAllProfiles_Should_ReturnFailed_WhenExceptionOccurs()
    {
        // Arrange
        await Context.DisposeAsync();
        var query = new GetAllProfilesQuery();
        var handler = new GetAllProfilesQueryHandler(Repository);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
    }
}

