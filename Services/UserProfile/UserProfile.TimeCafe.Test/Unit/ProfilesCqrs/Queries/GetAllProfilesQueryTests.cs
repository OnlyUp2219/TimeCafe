namespace UserProfile.TimeCafe.Test.Unit.ProfilesCqrs.Queries;

public class GetAllProfilesQueryTests : BaseCqrsTest
{
    [Fact]
    public async Task Handler_GetAllProfiles_Should_ReturnSuccess_WhenProfilesExist()
    {
        // Arrange
        await SeedProfileAsync("user1", "Иван", "Петров");
        await SeedProfileAsync("user2", "Мария", "Сидорова");
        await SeedProfileAsync("user3", "Алексей", "Смирнов");

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
