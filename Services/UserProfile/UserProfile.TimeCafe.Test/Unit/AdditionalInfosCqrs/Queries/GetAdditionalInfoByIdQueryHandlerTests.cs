namespace UserProfile.TimeCafe.Test.Unit.AdditionalInfosCqrs.Queries;

public class GetAdditionalInfoByIdQueryHandlerTests
{
    [Fact]
    public async Task Handle_Should_Return_Success_When_Exists()
    {
        var repoMock = new Mock<IAdditionalInfoRepository>();
        var info = new AdditionalInfo { InfoId = 11, UserId = "U1", InfoText = "txt", CreatedAt = DateTime.UtcNow };
        repoMock.Setup(r => r.GetAdditionalInfoByIdAsync(11, It.IsAny<CancellationToken>())).ReturnsAsync(info);
        var handler = new GetAdditionalInfoByIdQueryHandler(repoMock.Object);

        var result = await handler.Handle(new GetAdditionalInfoByIdQuery(11), CancellationToken.None);

        result.Success.Should().BeTrue();
        result.AdditionalInfo.Should().NotBeNull();
        result.AdditionalInfo!.InfoId.Should().Be(11);
        result.Code.Should().BeNull();
    }

    [Fact]
    public void Validator_Should_Pass_For_Valid_Id()
    {
        var validator = new GetAdditionalInfoByIdQueryValidator();
        var q = new GetAdditionalInfoByIdQuery(5);
        validator.Validate(q).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validator_Should_Fail_For_Invalid_Id()
    {
        var validator = new GetAdditionalInfoByIdQueryValidator();
        var q = new GetAdditionalInfoByIdQuery(0);
        validator.Validate(q).IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_Should_Return_NotFound_When_No_Info()
    {
        var repoMock = new Mock<IAdditionalInfoRepository>();
        repoMock.Setup(r => r.GetAdditionalInfoByIdAsync(12, It.IsAny<CancellationToken>())).ReturnsAsync((AdditionalInfo?)null);
        var handler = new GetAdditionalInfoByIdQueryHandler(repoMock.Object);

        var result = await handler.Handle(new GetAdditionalInfoByIdQuery(12), CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be("AdditionalInfoNotFound");
    }

    [Fact]
    public async Task Handle_Should_Return_Failed_On_Exception()
    {
        var repoMock = new Mock<IAdditionalInfoRepository>();
        repoMock.Setup(r => r.GetAdditionalInfoByIdAsync(13, It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("boom"));
        var handler = new GetAdditionalInfoByIdQueryHandler(repoMock.Object);

        var result = await handler.Handle(new GetAdditionalInfoByIdQuery(13), CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be("GetAdditionalInfoFailed");
    }
}
