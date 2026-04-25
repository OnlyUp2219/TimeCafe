using UserProfile.TimeCafe.Domain.DTOs;

namespace UserProfile.TimeCafe.Test.Unit.PhotosCqrs.Queries;

public class GetProfilePhotoQueryTests : BaseCqrsTest
{
    private readonly Mock<IProfilePhotoStorage> _storageMock;

    public GetProfilePhotoQueryTests()
    {
        _storageMock = new Mock<IProfilePhotoStorage>();
    }

    [Fact]
    public async Task Handler_GetPhoto_Should_ReturnSuccess_WhenPhotoExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var stream = new MemoryStream(PhotoTestData.TestPhotoBytes);
        _storageMock.Setup(s => s.GetAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PhotoStreamDto(stream, PhotoTestData.JpegContentType));

        var query = new GetProfilePhotoQuery(userId);
        var handler = new GetProfilePhotoQueryHandler(_storageMock.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Stream.Should().NotBeNull();
        result.Value.ContentType.Should().Be(PhotoTestData.JpegContentType);
    }

    [Fact]
    public async Task Handler_GetPhoto_Should_ReturnNotFound_WhenPhotoDoesNotExist()
    {
        // Arrange
        var userId = Guid.Parse(NonExistingUsers.UserId1);
        _storageMock.Setup(s => s.GetAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PhotoStreamDto?)null);

        var query = new GetProfilePhotoQuery(userId);
        var handler = new GetProfilePhotoQueryHandler(_storageMock.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task Handler_GetPhoto_Should_ReturnFailed_WhenExceptionOccurs()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _storageMock.Setup(s => s.GetAsync(userId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Storage error"));

        var query = new GetProfilePhotoQuery(userId);
        var handler = new GetProfilePhotoQueryHandler(_storageMock.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();

    }

    [Fact]
    public async Task Validator_Should_FailValidation_WhenUserIdEmpty()
    {
        // Arrange
        var query = new GetProfilePhotoQuery(Guid.Empty);
        var validator = new GetProfilePhotoQueryValidator();

        // Act
        var result = await validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("image/png")]
    [InlineData("image/webp")]
    [InlineData("image/gif")]
    public async Task Handler_GetPhoto_Should_ReturnCorrectContentType(string contentType)
    {
        // Arrange
        var userId = Guid.NewGuid();
        var stream = new MemoryStream(PhotoTestData.SmallPhotoBytes);
        _storageMock.Setup(s => s.GetAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PhotoStreamDto(stream, contentType));

        var query = new GetProfilePhotoQuery(userId);
        var handler = new GetProfilePhotoQueryHandler(_storageMock.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.ContentType.Should().Be(contentType);
    }
}



