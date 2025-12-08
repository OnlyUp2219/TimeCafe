using UserProfile.TimeCafe.Application.CQRS.Photos.Commands;
using Microsoft.Extensions.Options;
using UserProfile.TimeCafe.Domain.DTOs;

namespace UserProfile.TimeCafe.Test.Unit.PhotosCqrs.Commands;

public class UploadProfilePhotoCommandTests : BaseCqrsTest
{
    private readonly Mock<IProfilePhotoStorage> _storageMock;
    private readonly Mock<IPhotoModerationService> _moderationMock;
    private readonly Mock<ILogger<UploadProfilePhotoCommandHandler>> _loggerMock;
    private readonly PhotoOptions _photoOptions;

    public UploadProfilePhotoCommandTests()
    {
        _storageMock = new Mock<IProfilePhotoStorage>();
        _moderationMock = new Mock<IPhotoModerationService>();
        _moderationMock.Setup(m => m.ModeratePhotoAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ModerationResult(true, null, null));
        _loggerMock = new Mock<ILogger<UploadProfilePhotoCommandHandler>>();
        _photoOptions = new PhotoOptions
        {
            AllowedContentTypes = ["image/jpeg", "image/png", "image/webp", "image/gif"],
            MaxSizeBytes = 5 * 1024 * 1024,
            PresignedUrlExpirationMinutes = 15
        };
    }

    [Fact]
    public async Task Handler_UploadPhoto_Should_ReturnSuccess_WhenValidData()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var profile = await SeedProfileAsync(userId, "Иван", "Петров");
        var stream = new MemoryStream([1, 2, 3, 4, 5]);
        var command = new UploadProfilePhotoCommand(userId.ToString(), stream, "image/jpeg", "photo.jpg", 5);

        _storageMock.Setup(s => s.UploadAsync(
                userId,
                It.IsAny<Stream>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PhotoUploadDto(true, $"profiles/{userId}/photo", "https://example.com/photo.jpg", 5, "image/jpeg"));

        var handler = new UploadProfilePhotoCommandHandler(_storageMock.Object, Repository, _moderationMock.Object, _loggerMock.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.StatusCode.Should().Be(201);
        result.Message.Should().Be("Фото загружено");
        result.Key.Should().Be($"profiles/{userId}/photo");
        result.Url.Should().Be("https://example.com/photo.jpg");
        result.Size.Should().Be(5);
        result.ContentType.Should().Be("image/jpeg");

        // Verify profile was updated
        var updatedProfile = await Repository.GetProfileByIdAsync(userId, CancellationToken.None);
        updatedProfile!.PhotoUrl.Should().Be("https://example.com/photo.jpg");
    }

    [Fact]
    public async Task Handler_UploadPhoto_Should_ReturnProfileNotFound_WhenProfileDoesNotExist()
    {
        // Arrange
        var stream = new MemoryStream([1, 2, 3, 4, 5]);
        var userId = Guid.NewGuid();
        var command = new UploadProfilePhotoCommand(userId.ToString(), stream, "image/jpeg", "photo.jpg", 5);
        var handler = new UploadProfilePhotoCommandHandler(_storageMock.Object, Repository, _moderationMock.Object, _loggerMock.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Code.Should().Be("ProfileNotFound");
        result.StatusCode.Should().Be(404);
        result.Message.Should().Be("Профиль не найден");

        // Verify storage was not called
        _storageMock.Verify(s => s.UploadAsync(
            It.IsAny<Guid>(),
            It.IsAny<Stream>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handler_UploadPhoto_Should_ReturnFailed_WhenStorageFails()
    {
        // Arrange
        var userId = Guid.NewGuid();
        await SeedProfileAsync(userId, "Иван", "Петров");
        var stream = new MemoryStream([1, 2, 3, 4, 5]);
        var command = new UploadProfilePhotoCommand(userId.ToString(), stream, "image/jpeg", "photo.jpg", 5);

        _storageMock.Setup(s => s.UploadAsync(
                It.IsAny<Guid>(),
                It.IsAny<Stream>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PhotoUploadDto(false));

        var handler = new UploadProfilePhotoCommandHandler(_storageMock.Object, Repository, _moderationMock.Object, _loggerMock.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Code.Should().Be("UploadFailed");
        result.StatusCode.Should().Be(500);
        result.Message.Should().Be("Не удалось загрузить фото");
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Validator_Should_FailValidation_WhenUserIdEmpty(bool isEmpty)
    {
        // Arrange
        var stream = new MemoryStream([1, 2, 3]);
        string userId = isEmpty ? string.Empty : Guid.NewGuid().ToString();
        var command = new UploadProfilePhotoCommand(userId, stream, "image/jpeg", "photo.jpg", 3);
        var validator = new UploadProfilePhotoCommandValidator(Options.Create(_photoOptions));

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        if (isEmpty)
        {
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "UserId");
        }
        else
        {
            result.IsValid.Should().BeTrue();
        }
    }

    [Theory]
    [InlineData("image/bmp")]
    [InlineData("image/svg+xml")]
    [InlineData("application/pdf")]
    public async Task Validator_Should_FailValidation_WhenContentTypeNotAllowed(string contentType)
    {
        // Arrange
        var userId = Guid.NewGuid();
        var stream = new MemoryStream([1, 2, 3]);
        var command = new UploadProfilePhotoCommand(userId.ToString(), stream, contentType, "photo.jpg", 3);
        var validator = new UploadProfilePhotoCommandValidator(Options.Create(_photoOptions));

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ContentType");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(6 * 1024 * 1024)]
    public async Task Validator_Should_FailValidation_WhenSizeInvalid(long size)
    {
        // Arrange
        var userId = Guid.NewGuid();
        var stream = new MemoryStream([1, 2, 3]);
        var command = new UploadProfilePhotoCommand(userId.ToString(), stream, "image/jpeg", "photo.jpg", size);
        var validator = new UploadProfilePhotoCommandValidator(Options.Create(_photoOptions));

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Size");
    }

    [Theory]
    [InlineData("image/jpeg")]
    [InlineData("image/png")]
    [InlineData("image/webp")]
    [InlineData("image/gif")]
    public async Task Validator_Should_PassValidation_WhenAllowedContentType(string contentType)
    {
        // Arrange
        var userId = Guid.NewGuid();
        var stream = new MemoryStream([1, 2, 3]);
        var command = new UploadProfilePhotoCommand(userId.ToString(), stream, contentType, "photo.jpg", 1024);
        var validator = new UploadProfilePhotoCommandValidator(Options.Create(_photoOptions));

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Handler_UploadPhoto_Should_UpdateExistingPhotoUrl_WhenProfileAlreadyHasPhoto()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var profile = await SeedProfileAsync(userId, "Иван", "Петров");
        profile.PhotoUrl = "https://old-url.com/photo.jpg";
        await Context.SaveChangesAsync();

        var stream = new MemoryStream([1, 2, 3, 4, 5]);
        var command = new UploadProfilePhotoCommand(userId.ToString(), stream, "image/jpeg", "new-photo.jpg", 5);

        _storageMock.Setup(s => s.UploadAsync(
                It.IsAny<Guid>(),
                It.IsAny<Stream>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PhotoUploadDto(true, "profiles/new-photo", "https://new-url.com/photo.jpg", 5, "image/jpeg"));

        var handler = new UploadProfilePhotoCommandHandler(_storageMock.Object, Repository, _moderationMock.Object, _loggerMock.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        var updatedProfile = await Repository.GetProfileByIdAsync(userId, CancellationToken.None);
        updatedProfile!.PhotoUrl.Should().Be("https://new-url.com/photo.jpg");
    }
}
