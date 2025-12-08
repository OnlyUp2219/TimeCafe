namespace UserProfile.TimeCafe.Test.Unit.AdditionalInfosCqrs.Commands;

public class CreateAdditionalInfoCommandHandlerTests
{
    [Fact]
    public async Task Handle_Should_Create_Info_And_Return_Success_Result()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var infoId = Guid.NewGuid();
        var repoMock = new Mock<IAdditionalInfoRepository>();
        var command = new CreateAdditionalInfoCommand(userId, "Text info", "creator");
        repoMock.Setup(r => r.CreateAdditionalInfoAsync(It.IsAny<AdditionalInfo>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AdditionalInfo a, CancellationToken _) => { a.InfoId = infoId; return a; });
        var handler = new CreateAdditionalInfoCommandHandler(repoMock.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.AdditionalInfo.Should().NotBeNull();
        result.AdditionalInfo!.InfoId.Should().Be(infoId);
        result.Message.Should().Contain("успешно");
        repoMock.Verify(r => r.CreateAdditionalInfoAsync(It.IsAny<AdditionalInfo>(), It.IsAny<CancellationToken>()), Times.Once());
    }

    [Fact]
    public void Validator_Should_Pass_For_Valid_Data()
    {
        var validator = new CreateAdditionalInfoCommandValidator();
        var cmd = new CreateAdditionalInfoCommand(Guid.NewGuid(), "text", "creator");
        var result = validator.Validate(cmd);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validator_Should_Fail_When_UserId_Empty()
    {
        var validator = new CreateAdditionalInfoCommandValidator();
        var cmd = new CreateAdditionalInfoCommand(Guid.Empty, "text");
        var result = validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validator_Should_Fail_When_InfoText_Too_Long()
    {
        var validator = new CreateAdditionalInfoCommandValidator();
        var longText = new string('x', 2001);
        var cmd = new CreateAdditionalInfoCommand(Guid.NewGuid(), longText);
        var result = validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_Should_Return_Failed_When_Exception()
    {
        // Arrange
        var repoMock = new Mock<IAdditionalInfoRepository>();
        repoMock.Setup(r => r.CreateAdditionalInfoAsync(It.IsAny<AdditionalInfo>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("DB error"));
        var handler = new CreateAdditionalInfoCommandHandler(repoMock.Object);

        // Act
        var result = await handler.Handle(new CreateAdditionalInfoCommand(Guid.NewGuid(), "Txt"), CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Code.Should().Be("CreateAdditionalInfoFailed");
    }
}
