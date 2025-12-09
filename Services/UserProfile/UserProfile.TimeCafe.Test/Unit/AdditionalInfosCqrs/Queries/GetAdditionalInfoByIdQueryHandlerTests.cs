using static UserProfile.TimeCafe.Test.Integration.Helpers.TestData;

namespace UserProfile.TimeCafe.Test.Unit.AdditionalInfosCqrs.Queries;

public class GetAdditionalInfoByIdQueryHandlerTests
{
    [Fact]
    public async Task Handle_Should_Return_Success_When_Exists()
    {
        var infoId = Guid.Parse(AdditionalInfoData.Info1Id);
        var userId = Guid.Parse(ExistingUsers.User1Id);
        var repoMock = new Mock<IAdditionalInfoRepository>();
        var info = new AdditionalInfo { InfoId = infoId, UserId = userId, InfoText = TestInfoTexts.TestInfo, CreatedAt = DateTime.UtcNow };
        repoMock.Setup(r => r.GetAdditionalInfoByIdAsync(infoId, It.IsAny<CancellationToken>())).ReturnsAsync(info);
        var handler = new GetAdditionalInfoByIdQueryHandler(repoMock.Object);

        var result = await handler.Handle(new GetAdditionalInfoByIdQuery(infoId.ToString()), CancellationToken.None);

        result.Success.Should().BeTrue();
        result.AdditionalInfo.Should().NotBeNull();
        result.AdditionalInfo!.InfoId.Should().Be(infoId);
        result.Code.Should().BeNull();
    }

    [Fact]
    public void Validator_Should_Pass_For_Valid_Id()
    {
        var validator = new GetAdditionalInfoByIdQueryValidator();
        var q = new GetAdditionalInfoByIdQuery(AdditionalInfoData.Info1Id);
        validator.Validate(q).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validator_Should_Fail_For_Invalid_Id()
    {
        var validator = new GetAdditionalInfoByIdQueryValidator();
        var q = new GetAdditionalInfoByIdQuery(InvalidIds.EmptyString);
        validator.Validate(q).IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_Should_Return_NotFound_When_No_Info()
    {
        var infoId = Guid.Parse(NonExistingUsers.UserId1);
        var repoMock = new Mock<IAdditionalInfoRepository>();
        repoMock.Setup(r => r.GetAdditionalInfoByIdAsync(infoId, It.IsAny<CancellationToken>())).ReturnsAsync((AdditionalInfo?)null);
        var handler = new GetAdditionalInfoByIdQueryHandler(repoMock.Object);

        var result = await handler.Handle(new GetAdditionalInfoByIdQuery(infoId.ToString()), CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be("AdditionalInfoNotFound");
    }
    [Fact]
    public async Task Handle_Should_Return_Failed_On_Exception()
    {
        var infoId = Guid.Parse(AdditionalInfoData.Info1Id);
        var repoMock = new Mock<IAdditionalInfoRepository>();
        repoMock.Setup(r => r.GetAdditionalInfoByIdAsync(infoId, It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("boom"));
        var handler = new GetAdditionalInfoByIdQueryHandler(repoMock.Object);

        var result = await handler.Handle(new GetAdditionalInfoByIdQuery(infoId.ToString()), CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be("GetAdditionalInfoFailed");
    }
}
