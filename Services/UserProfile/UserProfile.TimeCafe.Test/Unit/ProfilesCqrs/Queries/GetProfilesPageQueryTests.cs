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
        var handler = new GetProfilesPageQueryHandler(Repository);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Profiles.Should().NotBeNull();
        result.Profiles!.Should().HaveCount(10);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.TotalCount.Should().Be(25);
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
        var handler = new GetProfilesPageQueryHandler(Repository);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Profiles.Should().NotBeNull();
        result.Profiles!.Should().HaveCount(5);
        result.PageNumber.Should().Be(2);
        result.PageSize.Should().Be(10);
        result.TotalCount.Should().Be(15);
    }

    [Fact]
    public async Task Handler_GetProfilesPage_Should_ReturnGetFailed_WhenExceptionOccurs()
    {
        // Arrange
        await Context.DisposeAsync();
        var query = new GetProfilesPageQuery(1, 10);
        var handler = new GetProfilesPageQueryHandler(Repository);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Code.Should().Be("GetProfilesPageFailed");
        result.StatusCode.Should().Be(500);
        result.Message.Should().Be("Не удалось получить страницу профилей");
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(-1, 10)]
    [InlineData(1, 0)]
    [InlineData(1, -1)]
    public async Task Validator_Should_FailValidation_WhenPageNumberOrSizeInvalid(int pageNumber, int pageSize)
    {
        // Arrange
        var query = new GetProfilesPageQuery(pageNumber, pageSize);
        var validator = new GetProfilesPageQueryValidator();

        // Act
        var result = await validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Validator_Should_FailValidation_WhenPageSizeExceedsLimit()
    {
        // Arrange
        var query = new GetProfilesPageQuery(1, 101);
        var validator = new GetProfilesPageQueryValidator();

        // Act
        var result = await validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "PageSize");
    }

    [Fact]
    public async Task Validator_Should_PassValidation_WhenPageSizeAt100()
    {
        // Arrange
        var query = new GetProfilesPageQuery(1, 100);
        var validator = new GetProfilesPageQueryValidator();

        // Act
        var result = await validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}
