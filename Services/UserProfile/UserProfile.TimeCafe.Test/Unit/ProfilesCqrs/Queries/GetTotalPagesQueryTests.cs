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
        result.Success.Should().BeTrue();
        result.TotalCount.Should().Be(42);
        result.Message.Should().Contain("Всего профилей: 42");
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
        result.Success.Should().BeTrue();
        result.TotalCount.Should().Be(0);
        result.Message.Should().Contain("Всего профилей: 0");
    }

    [Fact]
    public async Task Handler_GetTotalPages_Should_ThrowCqrsResultException_WhenExceptionOccurs()
    {
        // Arrange
        await Context.DisposeAsync();
        var query = new GetTotalPagesQuery();
        var handler = new GetTotalPagesQueryHandler(Repository);

        // Act
        var ex = await Assert.ThrowsAsync<CqrsResultException>(
            () => handler.Handle(query, CancellationToken.None));

        // Assert
        ex.Result.Should().NotBeNull();
        ex.Result!.Success.Should().BeFalse();
        ex.Result.Code.Should().Be("GetTotalPagesFailed");
        ex.Result.StatusCode.Should().Be(500);
        ex.Result.Message.Should().Be("Не удалось получить общее количество");
    }
}
