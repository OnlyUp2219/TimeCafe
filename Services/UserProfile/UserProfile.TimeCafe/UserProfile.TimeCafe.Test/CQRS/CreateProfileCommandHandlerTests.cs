using FluentAssertions;

using Microsoft.Extensions.Logging;

using Moq;

using UserProfile.TimeCafe.Application.CQRS.Profiles.Commands;
using UserProfile.TimeCafe.Domain.Contracts;
using UserProfile.TimeCafe.Domain.Models;

namespace UserProfile.TimeCafe.Test.CQRS;

public class CreateProfileCommandHandlerTests
{
    private readonly Mock<IUserRepositories> _repositoryMock;
    private readonly Mock<ILogger<CreateProfileCommandHandler>> _loggerMock;
    private readonly CreateProfileCommandHandler _handler;

    public CreateProfileCommandHandlerTests()
    {
        _repositoryMock = new Mock<IUserRepositories>();
        _loggerMock = new Mock<ILogger<CreateProfileCommandHandler>>();
        _handler = new CreateProfileCommandHandler(_repositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldCreateProfile_WhenValidCommandProvided()
    {
        // Arrange
        var command = new CreateProfileCommand("user123", "Иван", "Иванов", Gender.Male);
        var expectedProfile = new Profile
        {
            UserId = command.UserId,
            FirstName = command.FirstName,
            LastName = command.LastName,
            Gender = command.Gender,
            ProfileStatus = ProfileStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        _repositoryMock
            .Setup(r => r.CreateProfileAsync(It.IsAny<Profile>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedProfile);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.UserId.Should().Be(command.UserId);
        result.FirstName.Should().Be(command.FirstName);
        result.LastName.Should().Be(command.LastName);
        result.Gender.Should().Be(command.Gender);
        result.ProfileStatus.Should().Be(ProfileStatus.Pending);

        _repositoryMock.Verify(r => r.CreateProfileAsync(
            It.Is<Profile>(p =>
                p.UserId == command.UserId &&
                p.FirstName == command.FirstName &&
                p.LastName == command.LastName &&
                p.Gender == command.Gender &&
                p.ProfileStatus == ProfileStatus.Pending
            ),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldLogInformation_WhenCreatingProfile()
    {
        // Arrange
        var command = new CreateProfileCommand("user456", "Мария", "Петрова", Gender.Female);
        var profile = new Profile
        {
            UserId = command.UserId,
            FirstName = command.FirstName,
            LastName = command.LastName,
            Gender = command.Gender,
            ProfileStatus = ProfileStatus.Pending
        };

        _repositoryMock
            .Setup(r => r.CreateProfileAsync(It.IsAny<Profile>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Создание профиля через CQRS для UserId user456")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldSetCreatedAtToUtcNow_WhenCreatingProfile()
    {
        // Arrange
        var command = new CreateProfileCommand("user789", "Алексей", "Сидоров", Gender.Male);
        var beforeCreate = DateTime.UtcNow;

        Profile? capturedProfile = null;
        _repositoryMock
            .Setup(r => r.CreateProfileAsync(It.IsAny<Profile>(), It.IsAny<CancellationToken>()))
            .Callback<Profile, CancellationToken?>((p, ct) => capturedProfile = p)
            .ReturnsAsync((Profile p, CancellationToken? ct) => p);

        // Act
        await _handler.Handle(command, CancellationToken.None);
        var afterCreate = DateTime.UtcNow;

        // Assert
        capturedProfile.Should().NotBeNull();
        capturedProfile!.CreatedAt.Should().BeAfter(beforeCreate.AddSeconds(-1));
        capturedProfile.CreatedAt.Should().BeBefore(afterCreate.AddSeconds(1));
    }

    [Fact]
    public async Task Handle_ShouldReturnNull_WhenRepositoryReturnsNull()
    {
        // Arrange
        var command = new CreateProfileCommand("user999", "Тест", "Тестов", Gender.NotSpecified);

        _repositoryMock
            .Setup(r => r.CreateProfileAsync(It.IsAny<Profile>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Profile?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }
}
