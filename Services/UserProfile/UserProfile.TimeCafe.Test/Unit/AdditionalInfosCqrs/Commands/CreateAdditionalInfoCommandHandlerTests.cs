namespace UserProfile.TimeCafe.Test.Unit.AdditionalInfosCqrs.Commands;
 
public class CreateAdditionalInfoCommandHandlerTests
{
    private readonly Mock<IAdditionalInfoRepository> _repoMock = new();
    private readonly Mock<IUserRepositories> _userRepoMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly Mock<IPublisher> _publisherMock = new();

    public CreateAdditionalInfoCommandHandlerTests()
    {
        _uowMock.Setup(u => u.AdditionalInfos).Returns(_repoMock.Object);
        _uowMock.Setup(u => u.Profiles).Returns(_userRepoMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Create_Info_And_Return_Success_Result()
    {
        // Arrange
        var command = new CreateAdditionalInfoCommand(Guid.Parse(ExistingUsers.User1Id), TestInfoTexts.TestInfo, "creator");
        var profile = new Profile
        {
            UserId = Guid.Parse(ExistingUsers.User1Id),
            FirstName = ExistingUsers.User1FirstName,
            LastName = ExistingUsers.User1LastName
        };
        _userRepoMock.Setup(u => u.GetByIdAsync(Guid.Parse(ExistingUsers.User1Id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);
        _repoMock.Setup(r => r.CreateAsync(It.IsAny<AdditionalInfo>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AdditionalInfo a, CancellationToken _) => { a.InfoId = Guid.Parse(AdditionalInfoData.Info1Id); return a; });
        var handler = new CreateAdditionalInfoCommandHandler(_uowMock.Object, _publisherMock.Object);

        // Act
        var result = await handler.Handle(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.InfoId.Should().Be(Guid.Parse(AdditionalInfoData.Info1Id));
        _userRepoMock.Verify(u => u.GetByIdAsync(Guid.Parse(ExistingUsers.User1Id), It.IsAny<CancellationToken>()), Times.Once());
        _repoMock.Verify(r => r.CreateAsync(It.IsAny<AdditionalInfo>(), It.IsAny<CancellationToken>()), Times.Once());
        _publisherMock.Verify(p => p.Publish(It.Is<AdditionalInfoChangedEvent>(e => e.UserId == Guid.Parse(ExistingUsers.User1Id)), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void Validator_Should_Pass_For_Valid_Data()
    {
        var validator = new CreateAdditionalInfoCommandValidator();
        var cmd = new CreateAdditionalInfoCommand(Guid.Parse(ExistingUsers.User1Id), TestInfoTexts.TestInfo, "creator");
        var result = validator.Validate(cmd);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validator_Should_Fail_When_UserId_Empty()
    {
        var validator = new CreateAdditionalInfoCommandValidator();
        var cmd = new CreateAdditionalInfoCommand(Guid.Empty, TestInfoTexts.TestInfo);
        var result = validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validator_Should_Fail_When_InfoText_Too_Long()
    {
        var validator = new CreateAdditionalInfoCommandValidator();
        var longText = new string('x', 2001);
        var cmd = new CreateAdditionalInfoCommand(Guid.Parse(ExistingUsers.User1Id), longText);
        var result = validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_Should_Return_ProfileNotFound_When_Profile_Does_Not_Exist()
    {
        // Arrange
        var command = new CreateAdditionalInfoCommand(Guid.Parse(NonExistingUsers.UserId1), TestInfoTexts.TestInfo, "creator");
        _userRepoMock.Setup(u => u.GetByIdAsync(Guid.Parse(NonExistingUsers.UserId1), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Profile?)null);
        var handler = new CreateAdditionalInfoCommandHandler(_uowMock.Object, _publisherMock.Object);

        // Act
        var result = await handler.Handle(command);

        // Assert
        result.IsFailed.Should().BeTrue();
        _userRepoMock.Verify(u => u.GetByIdAsync(Guid.Parse(NonExistingUsers.UserId1), It.IsAny<CancellationToken>()), Times.Once());
        _repoMock.Verify(r => r.CreateAsync(It.IsAny<AdditionalInfo>(), It.IsAny<CancellationToken>()), Times.Never());
    }

    [Fact]
    public async Task Handle_Should_ReturnFailed_When_Exception()
    {
        // Arrange
        var profile = new Profile
        {
            UserId = Guid.Parse(ExistingUsers.User1Id),
            FirstName = ExistingUsers.User1FirstName,
            LastName = ExistingUsers.User1LastName
        };
        _userRepoMock.Setup(u => u.GetByIdAsync(Guid.Parse(ExistingUsers.User1Id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);
        _repoMock.Setup(r => r.CreateAsync(It.IsAny<AdditionalInfo>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("DB error"));
        var handler = new CreateAdditionalInfoCommandHandler(_uowMock.Object, _publisherMock.Object);

        // Act
        var result = await handler.Handle(new CreateAdditionalInfoCommand(Guid.Parse(ExistingUsers.User1Id), "Txt"));

        // Assert
        result.IsFailed.Should().BeTrue();
    }
}
