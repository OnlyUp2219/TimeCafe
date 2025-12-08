using UserProfile.TimeCafe.Application.CQRS.Photos.Queries;
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
        var stream = new MemoryStream([1, 2, 3, 4, 5]);
        _storageMock.Setup(s => s.GetAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PhotoStreamDto(stream, "image/jpeg"));

        var query = new GetProfilePhotoQuery(userId);
        var handler = new GetProfilePhotoQueryHandler(_storageMock.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Stream.Should().NotBeNull();
        result.ContentType.Should().Be("image/jpeg");
    }

    [Fact]
    public async Task Handler_GetPhoto_Should_ReturnNotFound_WhenPhotoDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _storageMock.Setup(s => s.GetAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PhotoStreamDto?)null);

        var query = new GetProfilePhotoQuery(userId);
        var handler = new GetProfilePhotoQueryHandler(_storageMock.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Code.Should().Be("PhotoNotFound");
        result.StatusCode.Should().Be(404);
        result.Message.Should().Be("Фото не найдено");
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
        result.Success.Should().BeFalse();
        result.Code.Should().Be("PhotoGetFailed");
        result.StatusCode.Should().Be(500);
        result.Message.Should().Be("Ошибка получения фото");
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
        var stream = new MemoryStream([1, 2, 3]);
        _storageMock.Setup(s => s.GetAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PhotoStreamDto(stream, contentType));

        var query = new GetProfilePhotoQuery(userId);
        var handler = new GetProfilePhotoQueryHandler(_storageMock.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.ContentType.Should().Be(contentType);
    }
}
