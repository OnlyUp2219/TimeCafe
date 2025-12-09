using static UserProfile.TimeCafe.Test.Integration.Helpers.TestData;

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
        result.Success.Should().BeTrue();
        result.Profiles.Should().NotBeNull();
        result.Profiles!.Should().HaveCount(3);
        result.Message.Should().Contain("Получено профилей: 3");
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
        result.Success.Should().BeTrue();
        result.Profiles.Should().NotBeNull();
        result.Profiles!.Should().BeEmpty();
        result.Message.Should().Contain("Получено профилей: 0");
    }

    [Fact]
    public async Task Handler_GetAllProfiles_Should_ReturnGetFailed_WhenExceptionOccurs()
    {
        // Arrange
        await Context.DisposeAsync();
        var query = new GetAllProfilesQuery();
        var handler = new GetAllProfilesQueryHandler(Repository);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Code.Should().Be("GetAllProfilesFailed");
        result.StatusCode.Should().Be(500);
        result.Message.Should().Be("Не удалось получить профили");
    }
}
