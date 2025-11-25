namespace UserProfile.TimeCafe.Test.Unit.AdditionalInfosCqrs.Commands;

public class DeleteAdditionalInfoCommandHandlerTests
{
    [Fact]
    public async Task Handle_Should_Delete_Info_When_Exists()
    {
        // Arrange
        var existing = new AdditionalInfo { InfoId = 7, UserId = "U1", InfoText = "Some text", CreatedAt = DateTime.UtcNow, CreatedBy = "creator" };
        var repoMock = new Mock<IAdditionalInfoRepository>();
        repoMock.Setup(r => r.GetAdditionalInfoByIdAsync(7, It.IsAny<CancellationToken>())).ReturnsAsync(existing);
        repoMock.Setup(r => r.DeleteAdditionalInfoAsync(7, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var cmd = new DeleteAdditionalInfoCommand(7);
        var handler = new DeleteAdditionalInfoCommandHandler(repoMock.Object);

        // Act
        var result = await handler.Handle(cmd, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Code.Should().BeNull();
        repoMock.Verify(r => r.DeleteAdditionalInfoAsync(7, It.IsAny<CancellationToken>()), Times.Once());
    }

    [Fact]
    public void Validator_Should_Pass_For_Valid_Id()
    {
        var validator = new DeleteAdditionalInfoCommandValidator();
        var cmd = new DeleteAdditionalInfoCommand(10);
        validator.Validate(cmd).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validator_Should_Fail_For_Invalid_Id()
    {
        var validator = new DeleteAdditionalInfoCommandValidator();
        var cmd = new DeleteAdditionalInfoCommand(0);
        validator.Validate(cmd).IsValid.Should().BeFalse();
    }
    [Fact]
    public async Task Handle_Should_Return_NotFound_When_Not_Exist()
    {
        // Arrange
        var repoMock = new Mock<IAdditionalInfoRepository>();
        repoMock.Setup(r => r.GetAdditionalInfoByIdAsync(7, It.IsAny<CancellationToken>())).ReturnsAsync((AdditionalInfo?)null);
        var cmd = new DeleteAdditionalInfoCommand(7);
        var handler = new DeleteAdditionalInfoCommandHandler(repoMock.Object);

        // Act
        var result = await handler.Handle(cmd, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Code.Should().Be("AdditionalInfoNotFound");
    }
}
