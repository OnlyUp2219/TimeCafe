namespace UserProfile.TimeCafe.Test.Unit.AdditionalInfosCqrs.Commands;

public class CreateAdditionalInfoCommandHandlerTests
{
    [Fact]
    public async Task Handle_Should_Create_Info_And_Return_Success_Result()
    {
        // Arrange
        var repoMock = new Mock<IAdditionalInfoRepository>();
        var userRepoMock = new Mock<IUserRepositories>();
        var command = new CreateAdditionalInfoCommand(ExistingUsers.User1Id, TestInfoTexts.TestInfo, "creator");
        var profile = new Profile
        {
            UserId = Guid.Parse(ExistingUsers.User1Id),
            FirstName = ExistingUsers.User1FirstName,
            LastName = ExistingUsers.User1LastName
        };
        userRepoMock.Setup(u => u.GetProfileByIdAsync(Guid.Parse(ExistingUsers.User1Id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);
        repoMock.Setup(r => r.CreateAdditionalInfoAsync(It.IsAny<AdditionalInfo>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AdditionalInfo a, CancellationToken _) => { a.InfoId = Guid.Parse(AdditionalInfoData.Info1Id); return a; });
        var handler = new CreateAdditionalInfoCommandHandler(repoMock.Object, userRepoMock.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.AdditionalInfo.Should().NotBeNull();
        result.AdditionalInfo!.InfoId.Should().Be(Guid.Parse(AdditionalInfoData.Info1Id));
        result.Message.Should().Contain("успешно");
        userRepoMock.Verify(u => u.GetProfileByIdAsync(Guid.Parse(ExistingUsers.User1Id), It.IsAny<CancellationToken>()), Times.Once());
        repoMock.Verify(r => r.CreateAdditionalInfoAsync(It.IsAny<AdditionalInfo>(), It.IsAny<CancellationToken>()), Times.Once());
    }

    [Fact]
    public void Validator_Should_Pass_For_Valid_Data()
    {
        var validator = new CreateAdditionalInfoCommandValidator();
        var cmd = new CreateAdditionalInfoCommand(ExistingUsers.User1Id, TestInfoTexts.TestInfo, "creator");
        var result = validator.Validate(cmd);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validator_Should_Fail_When_UserId_Empty()
    {
        var validator = new CreateAdditionalInfoCommandValidator();
        var cmd = new CreateAdditionalInfoCommand(InvalidIds.EmptyString, TestInfoTexts.TestInfo);
        var result = validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validator_Should_Fail_When_InfoText_Too_Long()
    {
        var validator = new CreateAdditionalInfoCommandValidator();
        var longText = new string('x', 2001);
        var cmd = new CreateAdditionalInfoCommand(ExistingUsers.User1Id, longText);
        var result = validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_Should_Return_ProfileNotFound_When_Profile_Does_Not_Exist()
    {
        // Arrange
        var repoMock = new Mock<IAdditionalInfoRepository>();
        var userRepoMock = new Mock<IUserRepositories>();
        var command = new CreateAdditionalInfoCommand(NonExistingUsers.UserId1, TestInfoTexts.TestInfo, "creator");
        userRepoMock.Setup(u => u.GetProfileByIdAsync(Guid.Parse(NonExistingUsers.UserId1), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Profile?)null);
        var handler = new CreateAdditionalInfoCommandHandler(repoMock.Object, userRepoMock.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Code.Should().Be("ProfileNotFound");
        result.StatusCode.Should().Be(404);
        userRepoMock.Verify(u => u.GetProfileByIdAsync(Guid.Parse(NonExistingUsers.UserId1), It.IsAny<CancellationToken>()), Times.Once());
        repoMock.Verify(r => r.CreateAdditionalInfoAsync(It.IsAny<AdditionalInfo>(), It.IsAny<CancellationToken>()), Times.Never());
    }

    [Fact]
    public async Task Handle_Should_Return_Failed_When_Exception()
    {
        // Arrange
        var repoMock = new Mock<IAdditionalInfoRepository>();
        var userRepoMock = new Mock<IUserRepositories>();
        var profile = new Profile
        {
            UserId = Guid.Parse(ExistingUsers.User1Id),
            FirstName = ExistingUsers.User1FirstName,
            LastName = ExistingUsers.User1LastName
        };
        userRepoMock.Setup(u => u.GetProfileByIdAsync(Guid.Parse(ExistingUsers.User1Id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);
        repoMock.Setup(r => r.CreateAdditionalInfoAsync(It.IsAny<AdditionalInfo>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("DB error"));
        var handler = new CreateAdditionalInfoCommandHandler(repoMock.Object, userRepoMock.Object);

        // Act
        var result = await handler.Handle(new CreateAdditionalInfoCommand(ExistingUsers.User1Id, "Txt"), CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Code.Should().Be("CreateAdditionalInfoFailed");
    }
}
