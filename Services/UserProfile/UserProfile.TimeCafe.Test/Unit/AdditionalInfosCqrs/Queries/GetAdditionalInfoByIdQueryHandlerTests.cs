namespace UserProfile.TimeCafe.Test.Unit.AdditionalInfosCqrs.Queries;

public class GetAdditionalInfoByIdQueryHandlerTests
{
    [Fact]
    public async Task Handle_Should_Return_Success_When_Exists()
    {
        var infoId = Guid.Parse(AdditionalInfoData.Info1Id);
        var userId = Guid.Parse(ExistingUsers.User1Id);
        var repoMock = new Mock<IAdditionalInfoRepository>();
        var info = new AdditionalInfo { InfoId = infoId, UserId = userId, InfoText = TestInfoTexts.TestInfo, CreatedAt = DateTimeOffset.UtcNow };
        repoMock.Setup(r => r.GetAdditionalInfoByIdAsync(infoId, It.IsAny<CancellationToken>())).ReturnsAsync(info);
        var handler = new GetAdditionalInfoByIdQueryHandler(repoMock.Object);

        var result = await handler.Handle(new GetAdditionalInfoByIdQuery(infoId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.InfoId.Should().Be(infoId);

    }

    [Fact]
    public void Validator_Should_Pass_For_Valid_Id()
    {
        var validator = new GetAdditionalInfoByIdQueryValidator();
        var q = new GetAdditionalInfoByIdQuery(Guid.Parse(AdditionalInfoData.Info1Id));
        validator.Validate(q).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validator_Should_Fail_For_Invalid_Id()
    {
        var validator = new GetAdditionalInfoByIdQueryValidator();
        var q = new GetAdditionalInfoByIdQuery(Guid.Empty);
        validator.Validate(q).IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_Should_Return_NotFound_When_No_Info()
    {
        var infoId = Guid.Parse(NonExistingUsers.UserId1);
        var repoMock = new Mock<IAdditionalInfoRepository>();
        repoMock.Setup(r => r.GetAdditionalInfoByIdAsync(infoId, It.IsAny<CancellationToken>())).ReturnsAsync((AdditionalInfo?)null);
        var handler = new GetAdditionalInfoByIdQueryHandler(repoMock.Object);

        var result = await handler.Handle(new GetAdditionalInfoByIdQuery(infoId), CancellationToken.None);

        result.IsFailed.Should().BeTrue();
    }
    [Fact]
    public async Task Handle_Should_ReturnFailed_On_Exception()
    {
        var infoId = Guid.Parse(AdditionalInfoData.Info1Id);
        var repoMock = new Mock<IAdditionalInfoRepository>();
        repoMock.Setup(r => r.GetAdditionalInfoByIdAsync(infoId, It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("boom"));
        var handler = new GetAdditionalInfoByIdQueryHandler(repoMock.Object);

        var result = await handler.Handle(new GetAdditionalInfoByIdQuery(infoId), CancellationToken.None);
        result.IsFailed.Should().BeTrue();
    }
}


