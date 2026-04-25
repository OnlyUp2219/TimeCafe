namespace UserProfile.TimeCafe.Test.Unit.ProfilesCqrs.Queries;

public class GetTotalPagesQueryTests : BaseCqrsTest
{
    [Fact]
    public async Task Handler_GetTotalPages_Should_ReturnSuccess_WhenProfilesExist()
    {
        // Arrange
        for (int i = 1; i <= 42; i++)
        {
            await SeedProfileAsync(Guid.NewGuid(), $"FirstName{i}", $"LastName{i}");
        }

        var query = new GetTotalPagesQuery();
        var handler = new GetTotalPagesQueryHandler(Repository);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public async Task Handler_GetTotalPages_Should_ReturnZero_WhenNoProfiles()
    {
        // Arrange
        var query = new GetTotalPagesQuery();
        var handler = new GetTotalPagesQueryHandler(Repository);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(0);
    }

    [Fact]
    public async Task Handler_GetTotalPages_Should_ReturnFailed_WhenExceptionOccurs()
    {
        // Arrange
        await Context.DisposeAsync();
        var query = new GetTotalPagesQuery();
        var handler = new GetTotalPagesQueryHandler(Repository);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();

    }
}


