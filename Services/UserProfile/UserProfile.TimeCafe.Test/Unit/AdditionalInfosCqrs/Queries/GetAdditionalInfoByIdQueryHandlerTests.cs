namespace UserProfile.TimeCafe.Test.Unit.AdditionalInfosCqrs.Queries;

public class GetAdditionalInfoByIdQueryHandlerTests
{
    private readonly Mock<IAdditionalInfoRepository> _repoMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();

    public GetAdditionalInfoByIdQueryHandlerTests()
    {
        _uowMock.Setup(u => u.AdditionalInfos).Returns(_repoMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Return_Success_When_Exists()
    {
        var infoId = Guid.Parse(AdditionalInfoData.Info1Id);
        var userId = Guid.Parse(ExistingUsers.User1Id);
        var info = new AdditionalInfo { InfoId = infoId, UserId = userId, InfoText = TestInfoTexts.TestInfo, CreatedAt = DateTimeOffset.UtcNow };
        _repoMock.Setup(r => r.GetByIdAsync(infoId, It.IsAny<CancellationToken>())).ReturnsAsync(info);
        var handler = new GetAdditionalInfoByIdQueryHandler(_uowMock.Object);

        var result = await handler.Handle(new GetAdditionalInfoByIdQuery(infoId));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.InfoId.Should().Be(infoId);
    }

    [Fact]
    public async Task Handle_Should_Return_NotFound_When_No_Info()
    {
        var infoId = Guid.Parse(NonExistingUsers.UserId1);
        _repoMock.Setup(r => r.GetByIdAsync(infoId, It.IsAny<CancellationToken>())).ReturnsAsync((AdditionalInfo?)null);
        var handler = new GetAdditionalInfoByIdQueryHandler(_uowMock.Object);

        var result = await handler.Handle(new GetAdditionalInfoByIdQuery(infoId));

        // Assert
        result.IsFailed.Should().BeTrue();
        result.HasError<InfoNotFoundError>().Should().BeTrue();
    }

    [Fact]
    public async Task Handle_Should_ReturnFailed_On_Exception()
    {
        var infoId = Guid.Parse(AdditionalInfoData.Info1Id);
        _repoMock.Setup(r => r.GetByIdAsync(infoId, It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("boom"));
        var handler = new GetAdditionalInfoByIdQueryHandler(_uowMock.Object);

        var result = await handler.Handle(new GetAdditionalInfoByIdQuery(infoId));
        result.IsFailed.Should().BeTrue();
    }
}
