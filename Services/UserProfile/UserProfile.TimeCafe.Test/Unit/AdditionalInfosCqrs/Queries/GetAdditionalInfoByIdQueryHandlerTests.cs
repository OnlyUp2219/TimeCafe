namespace UserProfile.TimeCafe.Test.Unit.AdditionalInfosCqrs.Queries;

public class GetAdditionalInfoByIdQueryHandlerTests
{
    [Fact]
    public async Task Handle_Should_Return_Success_When_Exists()
    {
        var infoId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var repoMock = new Mock<IAdditionalInfoRepository>();
        var info = new AdditionalInfo { InfoId = infoId, UserId = userId, InfoText = "txt", CreatedAt = DateTime.UtcNow };
        repoMock.Setup(r => r.GetAdditionalInfoByIdAsync(infoId, It.IsAny<CancellationToken>())).ReturnsAsync(info);
        var handler = new GetAdditionalInfoByIdQueryHandler(repoMock.Object);

        var result = await handler.Handle(new GetAdditionalInfoByIdQuery(infoId), CancellationToken.None);

        result.Success.Should().BeTrue();
        result.AdditionalInfo.Should().NotBeNull();
        result.AdditionalInfo!.InfoId.Should().Be(infoId);
        result.Code.Should().BeNull();
    }

    [Fact]
    public void Validator_Should_Pass_For_Valid_Id()
    {
        var validator = new GetAdditionalInfoByIdQueryValidator();
        var q = new GetAdditionalInfoByIdQuery(Guid.NewGuid());
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
        var infoId = Guid.NewGuid();
        var repoMock = new Mock<IAdditionalInfoRepository>();
        repoMock.Setup(r => r.GetAdditionalInfoByIdAsync(infoId, It.IsAny<CancellationToken>())).ReturnsAsync((AdditionalInfo?)null);
        var handler = new GetAdditionalInfoByIdQueryHandler(repoMock.Object);

        var result = await handler.Handle(new GetAdditionalInfoByIdQuery(infoId), CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be("AdditionalInfoNotFound");
    }
    [Fact]
    public async Task Handle_Should_Return_Failed_On_Exception()
    {
        var infoId = Guid.NewGuid();
        var repoMock = new Mock<IAdditionalInfoRepository>();
        repoMock.Setup(r => r.GetAdditionalInfoByIdAsync(infoId, It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("boom"));
        var handler = new GetAdditionalInfoByIdQueryHandler(repoMock.Object);

        var result = await handler.Handle(new GetAdditionalInfoByIdQuery(infoId), CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be("GetAdditionalInfoFailed");
    }
}
