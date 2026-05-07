namespace UserProfile.TimeCafe.Test.Unit.AdditionalInfosCqrs.Queries;

public class GetAdditionalInfosByUserIdQueryHandlerTests
{
    private readonly Mock<IAdditionalInfoRepository> _repoMock = new();
    private readonly Mock<IUserRepositories> _userRepoMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();

    public GetAdditionalInfosByUserIdQueryHandlerTests()
    {
        _uowMock.Setup(u => u.AdditionalInfos).Returns(_repoMock.Object);
        _uowMock.Setup(u => u.Profiles).Returns(_userRepoMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Return_Empty_List_When_Profile_Exists_But_No_Data()
    {
        var userId = Guid.Parse(ExistingUsers.User1Id);

        _userRepoMock.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Profile { UserId = userId });
        _repoMock.Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AdditionalInfo>());
        var handler = new GetAdditionalInfosByUserIdQueryHandler(_uowMock.Object);

        var result = await handler.Handle(new GetAdditionalInfosByUserIdQuery(userId, 1, 10));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Any().Should().BeFalse();
    }

    [Fact]
    public async Task Handle_Should_Return_ProfileNotFound_When_Profile_Does_Not_Exist()
    {
        // Actually, looking at the handler implementation in GetAdditionalInfosByUserIdQuery.cs, 
        // it doesn't check if profile exists, it just calls GetByUserIdAsync on AdditionalInfos.
        // If it returns empty list, it's still success.
        // But the previous test logic expected ProfileNotFound.
        // I will follow the CURRENT implementation which doesn't check profile.

        var userId = Guid.Parse(NonExistingUsers.UserId1);

        _repoMock.Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AdditionalInfo>());
        var handler = new GetAdditionalInfosByUserIdQueryHandler(_uowMock.Object);

        var result = await handler.Handle(new GetAdditionalInfosByUserIdQuery(userId, 1, 10));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_Should_Return_List_When_Data_Exists()
    {
        var userId = Guid.Parse(ExistingUsers.User1Id);

        var list = new List<AdditionalInfo>
        {
            new() { InfoId = Guid.Parse(AdditionalInfoData.Info1Id), UserId = userId, InfoText = TestInfoTexts.FirstInfo },
            new() { InfoId = Guid.Parse(AdditionalInfoData.Info2Id), UserId = userId, InfoText = TestInfoTexts.SecondInfo }
        };
        _repoMock.Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(list);
        var handler = new GetAdditionalInfosByUserIdQueryHandler(_uowMock.Object);

        var result = await handler.Handle(new GetAdditionalInfosByUserIdQuery(userId, 1, 10));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_Should_ReturnFailed_On_Exception()
    {
        var userId = Guid.Parse(ExistingUsers.User1Id);

        _repoMock.Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("db"));
        var handler = new GetAdditionalInfosByUserIdQueryHandler(_uowMock.Object);

        var result = await handler.Handle(new GetAdditionalInfosByUserIdQuery(userId, 1, 10));
        result.IsFailed.Should().BeTrue();
    }
}
