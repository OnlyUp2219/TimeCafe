namespace UserProfile.TimeCafe.Test.Unit.AdditionalInfosCqrs.Commands;

public class UpdateAdditionalInfoCommandHandlerTests
{
    [Fact]
    public async Task Handle_Should_Update_Info_When_Exists()
    {
        // Arrange
        var infoId = Guid.Parse(AdditionalInfoData.Info1Id);
        var userId = Guid.Parse(ExistingUsers.User1Id);
        var existing = new AdditionalInfo { InfoId = infoId, UserId = userId, InfoText = TestInfoTexts.OriginalInfo, CreatedAt = DateTime.UtcNow, CreatedBy = "creator" };
        var repoMock = new Mock<IAdditionalInfoRepository>();
        repoMock.Setup(r => r.GetAdditionalInfoByIdAsync(infoId, It.IsAny<CancellationToken>())).ReturnsAsync(existing);
        repoMock.Setup(r => r.UpdateAdditionalInfoAsync(It.IsAny<AdditionalInfo>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AdditionalInfo a, CancellationToken _) => a);
        var info = new AdditionalInfo { InfoId = infoId, UserId = userId, InfoText = TestInfoTexts.UpdatedInfo, CreatedBy = "creator2", CreatedAt = existing.CreatedAt };
        var cmd = new UpdateAdditionalInfoCommand(info.InfoId.ToString(), info.UserId.ToString(), info.InfoText, info.CreatedBy);
        var handler = new UpdateAdditionalInfoCommandHandler(repoMock.Object);

        // Act
        var result = await handler.Handle(cmd, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.AdditionalInfo.Should().NotBeNull();
        result.AdditionalInfo!.InfoText.Should().Be(TestInfoTexts.UpdatedInfo);
        repoMock.Verify(r => r.UpdateAdditionalInfoAsync(It.Is<AdditionalInfo>(a => a.InfoId == infoId), It.IsAny<CancellationToken>()), Times.Once());
    }

    [Fact]
    public void Validator_Should_Pass_For_Valid_Data()
    {
        var validator = new UpdateAdditionalInfoCommandValidator();
        var info = new AdditionalInfo { InfoId = Guid.Parse(AdditionalInfoData.Info1Id), UserId = Guid.Parse(ExistingUsers.User1Id), InfoText = TestInfoTexts.TestInfo };
        var cmd = new UpdateAdditionalInfoCommand(info.InfoId.ToString(), info.UserId.ToString(), info.InfoText, info.CreatedBy);
        var result = validator.Validate(cmd);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validator_Should_Fail_When_Info_Null()
    {
        var validator = new UpdateAdditionalInfoCommandValidator();
        var cmd = new UpdateAdditionalInfoCommand(null!, null!, null!, null);
        var result = validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validator_Should_Fail_When_InfoId_Invalid()
    {
        var validator = new UpdateAdditionalInfoCommandValidator();
        var cmd = new UpdateAdditionalInfoCommand(InvalidIds.EmptyString, ExistingUsers.User1Id, "txt", null);
        var result = validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_Should_Return_NotFound_When_Info_Not_Exist()
    {
        // Arrange
        var infoId = Guid.Parse(NonExistingUsers.UserId1);
        var userId = Guid.Parse(NonExistingUsers.UserId2);
        var repoMock = new Mock<IAdditionalInfoRepository>();
        repoMock.Setup(r => r.GetAdditionalInfoByIdAsync(infoId, It.IsAny<CancellationToken>())).ReturnsAsync((AdditionalInfo?)null);
        var info = new AdditionalInfo { InfoId = infoId, UserId = userId, InfoText = TestInfoTexts.UpdatedInfo, CreatedBy = "creator2", CreatedAt = DateTime.UtcNow };
        var cmd = new UpdateAdditionalInfoCommand(info.InfoId.ToString(), info.UserId.ToString(), info.InfoText, info.CreatedBy);
        var handler = new UpdateAdditionalInfoCommandHandler(repoMock.Object);

        // Act
        var result = await handler.Handle(cmd, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Code.Should().Be("AdditionalInfoNotFound");
    }
}
