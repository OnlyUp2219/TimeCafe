namespace UserProfile.TimeCafe.Test.Unit.AdditionalInfosCqrs.Commands;
 
public class DeleteAdditionalInfoCommandHandlerTests
{
    private readonly Mock<IAdditionalInfoRepository> _repoMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly Mock<IPublisher> _publisherMock = new();

    public DeleteAdditionalInfoCommandHandlerTests()
    {
        _uowMock.Setup(u => u.AdditionalInfos).Returns(_repoMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Delete_Info_When_Exists()
    {
        // Arrange
        var infoId = Guid.Parse(AdditionalInfoData.Info1Id);
        var userId = Guid.Parse(ExistingUsers.User1Id);
        var existing = new AdditionalInfo { InfoId = infoId, UserId = userId, InfoText = TestInfoTexts.TestInfo, CreatedAt = DateTimeOffset.UtcNow, CreatedBy = "creator" };
        
        _repoMock.Setup(r => r.GetByIdAsync(infoId, It.IsAny<CancellationToken>())).ReturnsAsync(existing);
        _repoMock.Setup(r => r.DeleteAsync(infoId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var cmd = new DeleteAdditionalInfoCommand(infoId);
        var handler = new DeleteAdditionalInfoCommandHandler(_uowMock.Object, _publisherMock.Object);

        // Act
        var result = await handler.Handle(cmd);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _repoMock.Verify(r => r.DeleteAsync(infoId, It.IsAny<CancellationToken>()), Times.Once());
        _publisherMock.Verify(p => p.Publish(It.Is<AdditionalInfoChangedEvent>(e => e.UserId == userId), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void Validator_Should_Pass_For_Valid_Id()
    {
        var validator = new DeleteAdditionalInfoCommandValidator();
        var cmd = new DeleteAdditionalInfoCommand(Guid.Parse(NonExistingUsers.UserId1));
        validator.Validate(cmd).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validator_Should_Fail_For_Invalid_Id()
    {
        var validator = new DeleteAdditionalInfoCommandValidator();
        var cmd = new DeleteAdditionalInfoCommand(Guid.Empty);
        validator.Validate(cmd).IsValid.Should().BeFalse();
    }
    
    [Fact]
    public async Task Handle_Should_Return_NotFound_When_Not_Exist()
    {
        // Arrange
        var infoId = Guid.Parse(NonExistingUsers.UserId1);
        
        _repoMock.Setup(r => r.GetByIdAsync(infoId, It.IsAny<CancellationToken>())).ReturnsAsync((AdditionalInfo?)null);
        var cmd = new DeleteAdditionalInfoCommand(infoId);
        var handler = new DeleteAdditionalInfoCommandHandler(_uowMock.Object, _publisherMock.Object);

        // Act
        var result = await handler.Handle(cmd);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.HasError<InfoNotFoundError>().Should().BeTrue();
    }
}
