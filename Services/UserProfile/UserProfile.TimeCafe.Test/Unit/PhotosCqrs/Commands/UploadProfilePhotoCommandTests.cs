using Microsoft.Extensions.Options;
using UserProfile.TimeCafe.Domain.DTOs;

namespace UserProfile.TimeCafe.Test.Unit.PhotosCqrs.Commands;

public class UploadProfilePhotoCommandTests : BaseCqrsTest
{
    private readonly Mock<IProfilePhotoStorage> _storageMock;
    private readonly Mock<IPhotoModerationService> _moderationMock;
    private readonly Mock<ILogger<UploadProfilePhotoCommandHandler>> _loggerMock;
    private readonly Mock<IOptionsSnapshot<PhotoOptions>> _photoOptionsMock;

    public UploadProfilePhotoCommandTests()
    {
        _storageMock = new Mock<IProfilePhotoStorage>();
        _moderationMock = new Mock<IPhotoModerationService>();
        _moderationMock.Setup(m => m.ModeratePhotoAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ModerationResult(true, null, null));
        _loggerMock = new Mock<ILogger<UploadProfilePhotoCommandHandler>>();
        _photoOptionsMock = new Mock<IOptionsSnapshot<PhotoOptions>>();
        _photoOptionsMock.Setup(o => o.Value).Returns(new PhotoOptions
        {
            AllowedContentTypes = ["image/jpeg", "image/png", "image/webp", "image/gif"],
            MaxSizeBytes = 5 * 1024 * 1024,
            PresignedUrlExpirationMinutes = 15
        });
    }

    [Fact]
    public async Task Handler_UploadPhoto_Should_ReturnSuccess_WhenValidData()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var profile = await SeedProfileAsync(userId, ExistingUsers.User1FirstName, ExistingUsers.User1LastName);
        var stream = new MemoryStream(PhotoTestData.TestPhotoBytes);
        var command = new UploadProfilePhotoCommand(userId, stream, PhotoTestData.JpegContentType, PhotoTestData.TestFileName, PhotoTestData.ValidPhotoSize);

        _storageMock.Setup(s => s.UploadAsync(
                userId,
                It.IsAny<Stream>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PhotoUploadDto(true, $"profiles/{userId}/photo", null, PhotoTestData.ValidPhotoSize, PhotoTestData.JpegContentType));

        var handler = new UploadProfilePhotoCommandHandler(_storageMock.Object, Repository, _moderationMock.Object, _loggerMock.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be($"/userprofile/S3/image/{userId}");

        // Verify profile was updated
        var updatedProfile = await Repository.GetProfileByIdAsync(userId, CancellationToken.None);
        updatedProfile!.PhotoUrl.Should().Be($"profiles/{userId}/photo");
    }

    [Fact]
    public async Task Handler_UploadPhoto_Should_ReturnProfileNotFound_WhenProfileDoesNotExist()
    {
        // Arrange
        var stream = new MemoryStream(PhotoTestData.TestPhotoBytes);
        var userId = Guid.Parse(NonExistingUsers.UserId1);
        var command = new UploadProfilePhotoCommand(userId, stream, PhotoTestData.JpegContentType, PhotoTestData.TestFileName, PhotoTestData.ValidPhotoSize);
        var handler = new UploadProfilePhotoCommandHandler(_storageMock.Object, Repository, _moderationMock.Object, _loggerMock.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
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
        await SeedProfileAsync(userId, ExistingUsers.User1FirstName, ExistingUsers.User1LastName);
        var stream = new MemoryStream(PhotoTestData.TestPhotoBytes);
        var command = new UploadProfilePhotoCommand(userId, stream, PhotoTestData.JpegContentType, PhotoTestData.TestFileName, PhotoTestData.ValidPhotoSize);

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
        result.IsFailed.Should().BeTrue();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Validator_Should_FailValidation_WhenUserIdEmpty(bool isEmpty)
    {
        // Arrange
        var stream = new MemoryStream(PhotoTestData.SmallPhotoBytes);
        Guid userId = isEmpty ? Guid.Empty : Guid.Parse(ExistingUsers.User1Id);
        var command = new UploadProfilePhotoCommand(userId, stream, PhotoTestData.JpegContentType, PhotoTestData.TestFileName, PhotoTestData.SmallPhotoBytes.Length);
        var validator = new UploadProfilePhotoCommandValidator(_photoOptionsMock.Object);

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
        var stream = new MemoryStream(PhotoTestData.SmallPhotoBytes);
        var command = new UploadProfilePhotoCommand(userId, stream, contentType, PhotoTestData.TestFileName, PhotoTestData.SmallPhotoBytes.Length);
        var validator = new UploadProfilePhotoCommandValidator(_photoOptionsMock.Object);

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
        var stream = new MemoryStream(PhotoTestData.SmallPhotoBytes);
        var command = new UploadProfilePhotoCommand(userId, stream, PhotoTestData.JpegContentType, PhotoTestData.TestFileName, size);
        var validator = new UploadProfilePhotoCommandValidator(_photoOptionsMock.Object);

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
        var stream = new MemoryStream(PhotoTestData.SmallPhotoBytes);
        var command = new UploadProfilePhotoCommand(userId, stream, contentType, PhotoTestData.TestFileName, PhotoTestData.SmallPhotoSize);
        var validator = new UploadProfilePhotoCommandValidator(_photoOptionsMock.Object);

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
        var profile = await SeedProfileAsync(userId, ExistingUsers.User1FirstName, ExistingUsers.User1LastName);
        profile.PhotoUrl = PhotoTestData.OldPhotoUrl;
        await Context.SaveChangesAsync();

        var stream = new MemoryStream(PhotoTestData.TestPhotoBytes);
        var command = new UploadProfilePhotoCommand(userId, stream, PhotoTestData.JpegContentType, "new-photo.jpg", PhotoTestData.ValidPhotoSize);

        _storageMock.Setup(s => s.UploadAsync(
                It.IsAny<Guid>(),
                It.IsAny<Stream>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PhotoUploadDto(true, "profiles/new-photo", null, PhotoTestData.ValidPhotoSize, PhotoTestData.JpegContentType));

        var handler = new UploadProfilePhotoCommandHandler(_storageMock.Object, Repository, _moderationMock.Object, _loggerMock.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var updatedProfile = await Repository.GetProfileByIdAsync(userId, CancellationToken.None);
        updatedProfile!.PhotoUrl.Should().Be("profiles/new-photo");
    }
}


