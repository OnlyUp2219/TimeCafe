namespace UserProfile.TimeCafe.Test.Unit.ProfilesCqrs.Queries;

public class GetProfilesPageQueryTests : BaseCqrsTest
{
    [Fact]
    public async Task Handler_GetProfilesPage_Should_ReturnSuccess_WhenValidPageRequest()
    {
        // Arrange
        for (int i = 1; i <= 25; i++)
        {
            await SeedProfileAsync(Guid.NewGuid(), $"FirstName{i}", $"LastName{i}");
        }

        var query = new GetProfilesPageQuery(1, 10);
        var handler = new GetProfilesPageQueryHandler(Uow);

        // Act
        var result = await handler.Handle(query);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Items.Should().HaveCount(10);
    }

    [Fact]
    public async Task Handler_GetProfilesPage_Should_ReturnPartialPage_WhenLastPage()
    {
        // Arrange
        for (int i = 1; i <= 15; i++)
        {
            await SeedProfileAsync(Guid.NewGuid(), $"FirstName{i}", $"LastName{i}");
        }

        var query = new GetProfilesPageQuery(2, 10);
        var handler = new GetProfilesPageQueryHandler(Uow);

        // Act
        var result = await handler.Handle(query);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Items.Should().HaveCount(5);
    }

    [Fact]
    public async Task Handler_GetProfilesPage_Should_ReturnFailed_WhenExceptionOccurs()
    {
        // Arrange
        await Context.DisposeAsync();
        var query = new GetProfilesPageQuery(1, 10);
        var handler = new GetProfilesPageQueryHandler(Uow);

        // Act
        var result = await handler.Handle(query);

        // Assert
        result.IsFailed.Should().BeTrue();
    }
}
