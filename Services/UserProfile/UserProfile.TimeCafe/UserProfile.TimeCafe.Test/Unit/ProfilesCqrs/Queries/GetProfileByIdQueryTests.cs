namespace UserProfile.TimeCafe.Test.Unit.ProfilesCqrs.Queries;

public class GetProfileByIdQueryTests : BaseCqrsTest
{
    [Fact]
    public async Task Handler_GetProfileById_Should_ReturnSuccess_WhenProfileExists()
    {
        // Arrange
        await SeedProfileAsync("user123", "Иван", "Петров");
        var query = new GetProfileByIdQuery("user123");
        var handler = new GetProfileByIdQueryHandler(Repository);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Профиль найден");
        result.Profile.Should().NotBeNull();
        result.Profile!.UserId.Should().Be("user123");
        result.Profile.FirstName.Should().Be("Иван");
        result.Profile.LastName.Should().Be("Петров");
    }

    [Fact]
    public async Task Handler_GetProfileById_Should_ReturnProfileNotFound_WhenProfileDoesNotExist()
    {
        // Arrange
        var query = new GetProfileByIdQuery("nonexistent");
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
    public async Task Handler_GetProfileById_Should_ReturnGetFailed_WhenExceptionOccurs()
    {
        // Arrange
        await Context.DisposeAsync();
        var query = new GetProfileByIdQuery("user123");
        var handler = new GetProfileByIdQueryHandler(Repository);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Code.Should().Be("GetProfileFailed");
        result.StatusCode.Should().Be(500);
        result.Message.Should().Be("Не удалось получить профиль");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Validator_Should_FailValidation_WhenUserIdIsEmpty(string? userId)
    {
        // Arrange
        var query = new GetProfileByIdQuery(userId!);
        var validator = new GetProfileByIdQueryValidator();

        // Act
        var result = await validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "UserId");
    }

    [Fact]
    public async Task Validator_Should_FailValidation_WhenUserIdIsTooLong()
    {
        // Arrange
        var query = new GetProfileByIdQuery(new string('a', 65));
        var validator = new GetProfileByIdQueryValidator();

        // Act
        var result = await validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "UserId");
    }
}
