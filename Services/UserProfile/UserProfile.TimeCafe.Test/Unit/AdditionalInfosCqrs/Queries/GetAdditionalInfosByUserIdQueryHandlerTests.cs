namespace UserProfile.TimeCafe.Test.Unit.AdditionalInfosCqrs.Queries;

public class GetAdditionalInfosByUserIdQueryHandlerTests
{
    [Fact]
    public async Task Handle_Should_Return_Empty_List_When_Profile_Exists_But_No_Data()
    {
        var userId = Guid.Parse(ExistingUsers.User1Id);
        var repoMock = new Mock<IAdditionalInfoRepository>();
        var userRepoMock = new Mock<IUserRepositories>();

        userRepoMock.Setup(r => r.GetProfileByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Profile { UserId = userId });
        repoMock.Setup(r => r.GetAdditionalInfosByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<AdditionalInfo>());
        var handler = new GetAdditionalInfosByUserIdQueryHandler(repoMock.Object, userRepoMock.Object);

        var result = await handler.Handle(new GetAdditionalInfosByUserIdQuery(userId.ToString()), CancellationToken.None);

        result.Success.Should().BeTrue();
        result.AdditionalInfos.Should().NotBeNull();
        result.AdditionalInfos!.Any().Should().BeFalse();
    }

    [Fact]
    public async Task Handle_Should_Return_ProfileNotFound_When_Profile_Does_Not_Exist()
    {
        var userId = Guid.Parse(NonExistingUsers.UserId1);
        var repoMock = new Mock<IAdditionalInfoRepository>();
        var userRepoMock = new Mock<IUserRepositories>();

        userRepoMock.Setup(r => r.GetProfileByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Profile?)null);
        var handler = new GetAdditionalInfosByUserIdQueryHandler(repoMock.Object, userRepoMock.Object);

        var result = await handler.Handle(new GetAdditionalInfosByUserIdQuery(userId.ToString()), CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be("ProfileNotFound");
        result.StatusCode.Should().Be(404);
        result.Message.Should().Be("Профиль не найден");
    }

    [Fact]
    public void Validator_Should_Pass_For_Valid_UserId()
    {
        var validator = new GetAdditionalInfosByUserIdQueryValidator();
        var q = new GetAdditionalInfosByUserIdQuery(ExistingUsers.User1Id);
        validator.Validate(q).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validator_Should_Fail_For_Empty_UserId()
    {
        var validator = new GetAdditionalInfosByUserIdQueryValidator();
        var q = new GetAdditionalInfosByUserIdQuery(InvalidIds.EmptyString);
        validator.Validate(q).IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_Should_Return_List_When_Data_Exists()
    {
        var userId = Guid.Parse(ExistingUsers.User1Id);
        var repoMock = new Mock<IAdditionalInfoRepository>();
        var userRepoMock = new Mock<IUserRepositories>();

        userRepoMock.Setup(r => r.GetProfileByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Profile { UserId = userId });
        var list = new List<AdditionalInfo>
        {
            new() { InfoId = Guid.Parse(AdditionalInfoData.Info1Id), UserId = userId, InfoText = TestInfoTexts.FirstInfo },
            new() { InfoId = Guid.Parse(AdditionalInfoData.Info2Id), UserId = userId, InfoText = TestInfoTexts.SecondInfo }
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
    public async Task Handle_Should_ThrowCqrsResultException_On_Exception()
    {
        var userId = Guid.Parse(ExistingUsers.User1Id);
        var repoMock = new Mock<IAdditionalInfoRepository>();
        var userRepoMock = new Mock<IUserRepositories>();

        userRepoMock.Setup(r => r.GetProfileByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("db"));
        var handler = new GetAdditionalInfosByUserIdQueryHandler(repoMock.Object, userRepoMock.Object);

        var ex = await Assert.ThrowsAsync<BuildingBlocks.Exceptions.CqrsResultException>(
            () => handler.Handle(new GetAdditionalInfosByUserIdQuery(userId.ToString()), CancellationToken.None));

        ex.Result.Should().NotBeNull();
        ex.Result!.Success.Should().BeFalse();
        ex.Result.Code.Should().Be("GetAdditionalInfosFailed");
        ex.Result.StatusCode.Should().Be(500);
    }
}
