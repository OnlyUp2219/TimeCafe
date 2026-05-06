namespace UserProfile.TimeCafe.Test.Unit.AdditionalInfosCqrs.Commands;
 
public class UpdateAdditionalInfoCommandHandlerTests
{
    private readonly Mock<IAdditionalInfoRepository> _repoMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly Mock<IPublisher> _publisherMock = new();

    public UpdateAdditionalInfoCommandHandlerTests()
    {
        _uowMock.Setup(u => u.AdditionalInfos).Returns(_repoMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Update_Info_When_Exists()
    {
        // Arrange
        var infoId = Guid.Parse(AdditionalInfoData.Info1Id);
        var userId = Guid.Parse(ExistingUsers.User1Id);
        var existing = new AdditionalInfo { InfoId = infoId, UserId = userId, InfoText = TestInfoTexts.OriginalInfo, CreatedAt = DateTimeOffset.UtcNow, CreatedBy = "creator" };
        
        _repoMock.Setup(r => r.GetByIdAsync(infoId, It.IsAny<CancellationToken>())).ReturnsAsync(existing);
        _repoMock.Setup(r => r.UpdateAsync(It.IsAny<AdditionalInfo>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AdditionalInfo a, CancellationToken _) => a);
        
        var info = new AdditionalInfo { InfoId = infoId, UserId = userId, InfoText = TestInfoTexts.UpdatedInfo, CreatedBy = "creator2", CreatedAt = existing.CreatedAt };
        var cmd = new UpdateAdditionalInfoCommand(info.InfoId, info.UserId, info.InfoText, info.CreatedBy);
        var handler = new UpdateAdditionalInfoCommandHandler(_uowMock.Object, _publisherMock.Object);

        // Act
        var result = await handler.Handle(cmd);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.InfoText.Should().Be(TestInfoTexts.UpdatedInfo);
        _repoMock.Verify(r => r.UpdateAsync(It.Is<AdditionalInfo>(a => a.InfoId == infoId), It.IsAny<CancellationToken>()), Times.Once());
        _publisherMock.Verify(p => p.Publish(It.Is<AdditionalInfoChangedEvent>(e => e.UserId == userId), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void Validator_Should_Pass_For_Valid_Data()
    {
        var validator = new UpdateAdditionalInfoCommandValidator();
        var info = new AdditionalInfo { InfoId = Guid.Parse(AdditionalInfoData.Info1Id), UserId = Guid.Parse(ExistingUsers.User1Id), InfoText = TestInfoTexts.TestInfo };
        var cmd = new UpdateAdditionalInfoCommand(info.InfoId, info.UserId, info.InfoText, info.CreatedBy);
        var result = validator.Validate(cmd);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validator_Should_Fail_When_Info_Null()
    {
        var validator = new UpdateAdditionalInfoCommandValidator();
        var cmd = new UpdateAdditionalInfoCommand(Guid.Empty, Guid.Empty, null!, null);
        var result = validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validator_Should_Fail_When_InfoId_Invalid()
    {
        var validator = new UpdateAdditionalInfoCommandValidator();
        var cmd = new UpdateAdditionalInfoCommand(Guid.Empty, Guid.Parse(ExistingUsers.User1Id), "txt", null);
        var result = validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_Should_Return_NotFound_When_Info_Not_Exist()
    {
        // Arrange
        var infoId = Guid.Parse(NonExistingUsers.UserId1);
        var userId = Guid.Parse(NonExistingUsers.UserId2);
        
        _repoMock.Setup(r => r.GetByIdAsync(infoId, It.IsAny<CancellationToken>())).ReturnsAsync((AdditionalInfo?)null);
        var info = new AdditionalInfo { InfoId = infoId, UserId = userId, InfoText = TestInfoTexts.UpdatedInfo, CreatedBy = "creator2", CreatedAt = DateTimeOffset.UtcNow };
        var cmd = new UpdateAdditionalInfoCommand(info.InfoId, info.UserId, info.InfoText, info.CreatedBy);
        var handler = new UpdateAdditionalInfoCommandHandler(_uowMock.Object, _publisherMock.Object);

        // Act
        var result = await handler.Handle(cmd);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.HasError<InfoNotFoundError>().Should().BeTrue();
    }
}
