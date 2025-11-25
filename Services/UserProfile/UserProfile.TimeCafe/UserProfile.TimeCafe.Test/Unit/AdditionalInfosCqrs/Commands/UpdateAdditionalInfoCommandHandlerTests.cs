namespace UserProfile.TimeCafe.Test.Unit.AdditionalInfosCqrs.Commands;

public class UpdateAdditionalInfoCommandHandlerTests
{
    [Fact]
    public async Task Handle_Should_Update_Info_When_Exists()
    {
        // Arrange

        var existing = new AdditionalInfo { InfoId = 5, UserId = "U1", InfoText = "Old text", CreatedAt = DateTime.UtcNow, CreatedBy = "creator" };
        var repoMock = new Mock<IAdditionalInfoRepository>();
        repoMock.Setup(r => r.GetAdditionalInfoByIdAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(existing);
        repoMock.Setup(r => r.UpdateAdditionalInfoAsync(It.IsAny<AdditionalInfo>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AdditionalInfo a, CancellationToken _) => a);
        var cmd = new UpdateAdditionalInfoCommand(new AdditionalInfo { InfoId = 5, UserId = "U1", InfoText = "New text", CreatedBy = "creator2", CreatedAt = existing.CreatedAt });
        var handler = new UpdateAdditionalInfoCommandHandler(repoMock.Object);

        // Act
        var result = await handler.Handle(cmd, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.AdditionalInfo.Should().NotBeNull();
        result.AdditionalInfo!.InfoText.Should().Be("New text");
        repoMock.Verify(r => r.UpdateAdditionalInfoAsync(It.Is<AdditionalInfo>(a => a.InfoId == 5), It.IsAny<CancellationToken>()), Times.Once());
    }

    [Fact]
    public void Validator_Should_Pass_For_Valid_Data()
    {
        var validator = new UpdateAdditionalInfoCommandValidator();
        var info = new AdditionalInfo { InfoId = 5, UserId = "U1", InfoText = "ok" };
        var cmd = new UpdateAdditionalInfoCommand(info);
        var result = validator.Validate(cmd);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validator_Should_Fail_When_Info_Null()
    {
        var validator = new UpdateAdditionalInfoCommandValidator();
        var cmd = new UpdateAdditionalInfoCommand(null!);
        var result = validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validator_Should_Fail_When_InfoId_Invalid()
    {
        var validator = new UpdateAdditionalInfoCommandValidator();
        var info = new AdditionalInfo { InfoId = 0, UserId = "U1", InfoText = "txt" };
        var cmd = new UpdateAdditionalInfoCommand(info);
        var result = validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_Should_Return_NotFound_When_Info_Not_Exist()
    {
        // Arrange
        var repoMock = new Mock<IAdditionalInfoRepository>();
        repoMock.Setup(r => r.GetAdditionalInfoByIdAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync((AdditionalInfo?)null);
        var cmd = new UpdateAdditionalInfoCommand(new AdditionalInfo { InfoId = 5, UserId = "U1", InfoText = "New text", CreatedBy = "creator2", CreatedAt = DateTime.UtcNow });
        var handler = new UpdateAdditionalInfoCommandHandler(repoMock.Object);

        // Act
        var result = await handler.Handle(cmd, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Code.Should().Be("AdditionalInfoNotFound");
    }
}
