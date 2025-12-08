namespace UserProfile.TimeCafe.Test.Unit.AdditionalInfosCqrs.Queries;

public class GetAdditionalInfosByUserIdQueryHandlerTests
{
    [Fact]
    public async Task Handle_Should_Return_Empty_List_When_No_Data()
    {
        var userId = Guid.NewGuid();
        var repoMock = new Mock<IAdditionalInfoRepository>();
        var userRepoMock = new Mock<IUserRepositories>();
        repoMock.Setup(r => r.GetAdditionalInfosByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<AdditionalInfo>());
        var handler = new GetAdditionalInfosByUserIdQueryHandler(repoMock.Object, userRepoMock.Object);

        var result = await handler.Handle(new GetAdditionalInfosByUserIdQuery(userId.ToString()), CancellationToken.None);

        result.Success.Should().BeTrue();
        result.AdditionalInfos.Should().NotBeNull();
        result.AdditionalInfos!.Any().Should().BeFalse();
    }

    [Fact]
    public void Validator_Should_Pass_For_Valid_UserId()
    {
        var validator = new GetAdditionalInfosByUserIdQueryValidator();
        var q = new GetAdditionalInfosByUserIdQuery(Guid.NewGuid().ToString());
        validator.Validate(q).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validator_Should_Fail_For_Empty_UserId()
    {
        var validator = new GetAdditionalInfosByUserIdQueryValidator();
        var q = new GetAdditionalInfosByUserIdQuery(string.Empty);
        validator.Validate(q).IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_Should_Return_List_When_Data_Exists()
    {
        var userId = Guid.NewGuid();
        var repoMock = new Mock<IAdditionalInfoRepository>();
        var userRepoMock = new Mock<IUserRepositories>();
        var list = new List<AdditionalInfo>
        {
            new AdditionalInfo { InfoId = Guid.NewGuid(), UserId = userId, InfoText = "A" },
            new AdditionalInfo { InfoId = Guid.NewGuid(), UserId = userId, InfoText = "B" }
        };
        repoMock.Setup(r => r.GetAdditionalInfosByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(list);
        var handler = new GetAdditionalInfosByUserIdQueryHandler(repoMock.Object, userRepoMock.Object);

        var result = await handler.Handle(new GetAdditionalInfosByUserIdQuery(userId.ToString()), CancellationToken.None);

        result.Success.Should().BeTrue();
        result.AdditionalInfos.Should().HaveCount(2);
        result.Code.Should().BeNull();
    }
    [Fact]
    public async Task Handle_Should_Return_Failed_On_Exception()
    {
        var userId = Guid.NewGuid();
        var repoMock = new Mock<IAdditionalInfoRepository>();
        var userRepoMock = new Mock<IUserRepositories>();
        repoMock.Setup(r => r.GetAdditionalInfosByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("db"));
        var handler = new GetAdditionalInfosByUserIdQueryHandler(repoMock.Object, userRepoMock.Object);

        var result = await handler.Handle(new GetAdditionalInfosByUserIdQuery(userId.ToString()), CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be("GetAdditionalInfosFailed");
    }
}
