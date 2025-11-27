using UserProfile.TimeCafe.Application.CQRS.Photos.Commands;

namespace UserProfile.TimeCafe.Test.Unit.PhotosCqrs.Commands;

public class DeleteProfilePhotoCommandTests : BaseCqrsTest
{
    private readonly Mock<IProfilePhotoStorage> _storageMock;

    public DeleteProfilePhotoCommandTests()
    {
        _storageMock = new Mock<IProfilePhotoStorage>();
    }

    [Fact]
    public async Task Handler_DeletePhoto_Should_ReturnSuccess_WhenPhotoExists()
    {
        // Arrange
        var profile = await SeedProfileAsync("user123", "Иван", "Петров");
        profile.PhotoUrl = "https://example.com/photo.jpg";
        await Context.SaveChangesAsync();

        _storageMock.Setup(s => s.DeleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var command = new DeleteProfilePhotoCommand("user123");
        var handler = new DeleteProfilePhotoCommandHandler(_storageMock.Object, Repository);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.StatusCode.Should().Be(204);
        result.Message.Should().Be("Фото удалено");

        // Verify profile PhotoUrl was cleared
        var updatedProfile = await Repository.GetProfileByIdAsync("user123", CancellationToken.None);
        updatedProfile!.PhotoUrl.Should().BeNull();
    }

    [Fact]
    public async Task Handler_DeletePhoto_Should_ReturnProfileNotFound_WhenProfileDoesNotExist()
    {
        // Arrange
        var command = new DeleteProfilePhotoCommand("nonexistent");
        var handler = new DeleteProfilePhotoCommandHandler(_storageMock.Object, Repository);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Code.Should().Be("ProfileNotFound");
        result.StatusCode.Should().Be(404);
        result.Message.Should().Be("Профиль не найден");

        // Verify storage was not called
        _storageMock.Verify(s => s.DeleteAsync(
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handler_DeletePhoto_Should_ReturnPhotoNotFound_WhenPhotoDoesNotExist()
    {
        // Arrange
        await SeedProfileAsync("user123", "Иван", "Петров");

        _storageMock.Setup(s => s.DeleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var command = new DeleteProfilePhotoCommand("user123");
        var handler = new DeleteProfilePhotoCommandHandler(_storageMock.Object, Repository);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Code.Should().Be("PhotoNotFound");
        result.StatusCode.Should().Be(404);
        result.Message.Should().Be("Фото не найдено");
    }

    [Fact]
    public async Task Handler_DeletePhoto_Should_ReturnFailed_WhenExceptionOccurs()
    {
        // Arrange
        var profile = await SeedProfileAsync("user123", "Иван", "Петров");
        profile.PhotoUrl = "https://example.com/photo.jpg";
        await Context.SaveChangesAsync();

        _storageMock.Setup(s => s.DeleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Storage error"));

        var command = new DeleteProfilePhotoCommand("user123");
        var handler = new DeleteProfilePhotoCommandHandler(_storageMock.Object, Repository);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Code.Should().Be("PhotoDeleteFailed");
        result.StatusCode.Should().Be(500);
        result.Message.Should().Be("Ошибка удаления фото");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Validator_Should_FailValidation_WhenUserIdEmpty(string userId)
    {
        // Arrange
        var command = new DeleteProfilePhotoCommand(userId!);
        var validator = new DeleteProfilePhotoCommandValidator();

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "UserId");
    }

    [Fact]
    public async Task Validator_Should_PassValidation_WhenUserIdValid()
    {
        // Arrange
        var command = new DeleteProfilePhotoCommand("user123");
        var validator = new DeleteProfilePhotoCommandValidator();

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Handler_DeletePhoto_Should_ClearPhotoUrl_EvenIfNull()
    {
        // Arrange
        var profile = await SeedProfileAsync("user123", "Иван", "Петров");
        profile.PhotoUrl = null;
        await Context.SaveChangesAsync();

        _storageMock.Setup(s => s.DeleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var command = new DeleteProfilePhotoCommand("user123");
        var handler = new DeleteProfilePhotoCommandHandler(_storageMock.Object, Repository);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        var updatedProfile = await Repository.GetProfileByIdAsync("user123", CancellationToken.None);
        updatedProfile!.PhotoUrl.Should().BeNull();
    }
}
