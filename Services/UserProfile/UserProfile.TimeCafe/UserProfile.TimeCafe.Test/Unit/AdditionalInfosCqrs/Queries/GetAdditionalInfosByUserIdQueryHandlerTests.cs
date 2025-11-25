namespace UserProfile.TimeCafe.Test.Unit.AdditionalInfosCqrs.Queries;

public class GetAdditionalInfosByUserIdQueryHandlerTests
{
    [Fact]
    public async Task Handle_Should_Return_Empty_List_When_No_Data()
    {
        var repoMock = new Mock<IAdditionalInfoRepository>();
        repoMock.Setup(r => r.GetAdditionalInfosByUserIdAsync("UEMPTY", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<AdditionalInfo>());
        var handler = new GetAdditionalInfosByUserIdQueryHandler(repoMock.Object);

        var result = await handler.Handle(new GetAdditionalInfosByUserIdQuery("UEMPTY"), CancellationToken.None);

        result.Success.Should().BeTrue();
        result.AdditionalInfos.Should().NotBeNull();
        result.AdditionalInfos!.Any().Should().BeFalse();
    }

    [Fact]
    public void Validator_Should_Pass_For_Valid_UserId()
    {
        var validator = new GetAdditionalInfosByUserIdQueryValidator();
        var q = new GetAdditionalInfosByUserIdQuery("U1");
        validator.Validate(q).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validator_Should_Fail_For_Empty_UserId()
    {
        var validator = new GetAdditionalInfosByUserIdQueryValidator();
        var q = new GetAdditionalInfosByUserIdQuery("");
        validator.Validate(q).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validator_Should_Fail_For_Too_Long_UserId()
    {
        var validator = new GetAdditionalInfosByUserIdQueryValidator();
        var longUserId = new string('u', 451);
        var q = new GetAdditionalInfosByUserIdQuery(longUserId);
        validator.Validate(q).IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_Should_Return_List_When_Data_Exists()
    {
        var repoMock = new Mock<IAdditionalInfoRepository>();
        var list = new List<AdditionalInfo>
        {
            new AdditionalInfo { InfoId = 1, UserId = "U1", InfoText = "A" },
            new AdditionalInfo { InfoId = 2, UserId = "U1", InfoText = "B" }
        };
        repoMock.Setup(r => r.GetAdditionalInfosByUserIdAsync("U1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(list);
        var handler = new GetAdditionalInfosByUserIdQueryHandler(repoMock.Object);

        var result = await handler.Handle(new GetAdditionalInfosByUserIdQuery("U1"), CancellationToken.None);

        result.Success.Should().BeTrue();
        result.AdditionalInfos.Should().HaveCount(2);
        result.Code.Should().BeNull();
    }

    [Fact]
    public async Task Handle_Should_Return_Failed_On_Exception()
    {
        var repoMock = new Mock<IAdditionalInfoRepository>();
        repoMock.Setup(r => r.GetAdditionalInfosByUserIdAsync("UERR", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("db"));
        var handler = new GetAdditionalInfosByUserIdQueryHandler(repoMock.Object);

        var result = await handler.Handle(new GetAdditionalInfosByUserIdQuery("UERR"), CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be("GetAdditionalInfosFailed");
    }
}
