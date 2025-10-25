using FluentAssertions;

using Microsoft.Extensions.Logging;

using Moq;

using UserProfile.TimeCafe.Application.CQRS.Profiles.Queries;
using UserProfile.TimeCafe.Domain.Contracts;
using UserProfile.TimeCafe.Domain.Models;

namespace UserProfile.TimeCafe.Test.CQRS;

public class GetProfileByIdQueryHandlerTests
{
    private readonly Mock<IUserRepositories> _repositoryMock;
    private readonly Mock<ILogger<GetProfileByIdQueryHandler>> _loggerMock;
    private readonly GetProfileByIdQueryHandler _handler;

    public GetProfileByIdQueryHandlerTests()
    {
        _repositoryMock = new Mock<IUserRepositories>();
        _loggerMock = new Mock<ILogger<GetProfileByIdQueryHandler>>();
        _handler = new GetProfileByIdQueryHandler(_repositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnProfile_WhenProfileExists()
    {
        // Arrange
        var userId = "user123";
        var query = new GetProfileByIdQuery(userId);
        var expectedProfile = new Profile
        {
            UserId = userId,
            FirstName = "Иван",
            LastName = "Иванов",
            Gender = Gender.Male,
            ProfileStatus = ProfileStatus.Completed,
            CreatedAt = DateTime.UtcNow
        };

        _repositoryMock
            .Setup(r => r.GetProfileByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedProfile);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.UserId.Should().Be(userId);
        result.FirstName.Should().Be("Иван");
        result.LastName.Should().Be("Иванов");
        result.Gender.Should().Be(Gender.Male);
        result.ProfileStatus.Should().Be(ProfileStatus.Completed);

        _repositoryMock.Verify(r => r.GetProfileByIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnNull_WhenProfileDoesNotExist()
    {
        // Arrange
        var userId = "nonexistent";
        var query = new GetProfileByIdQuery(userId);

        _repositoryMock
            .Setup(r => r.GetProfileByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Profile?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
        _repositoryMock.Verify(r => r.GetProfileByIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldLogDebug_WhenGettingProfile()
    {
        // Arrange
        var userId = "user456";
        var query = new GetProfileByIdQuery(userId);
        var profile = new Profile
        {
            UserId = userId,
            FirstName = "Мария",
            LastName = "Петрова",
            Gender = Gender.Female,
            ProfileStatus = ProfileStatus.Pending
        };

        _repositoryMock
            .Setup(r => r.GetProfileByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Получение профиля UserId={userId}")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldPassCancellationToken_ToRepository()
    {
        // Arrange
        var userId = "user789";
        var query = new GetProfileByIdQuery(userId);
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        _repositoryMock
            .Setup(r => r.GetProfileByIdAsync(userId, cancellationToken))
            .ReturnsAsync(new Profile { UserId = userId });

        // Act
        await _handler.Handle(query, cancellationToken);

        // Assert
        _repositoryMock.Verify(r => r.GetProfileByIdAsync(userId, cancellationToken), Times.Once);
    }

    [Theory]
    [InlineData("")]
    [InlineData("user-with-long-id-12345678")]
    [InlineData("special@char#user")]
    public async Task Handle_ShouldHandleDifferentUserIdFormats(string userId)
    {
        // Arrange
        var query = new GetProfileByIdQuery(userId);
        var profile = new Profile
        {
            UserId = userId,
            FirstName = "Test",
            LastName = "User",
            Gender = Gender.NotSpecified,
            ProfileStatus = ProfileStatus.Pending
        };

        _repositoryMock
            .Setup(r => r.GetProfileByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.UserId.Should().Be(userId);
    }
}
