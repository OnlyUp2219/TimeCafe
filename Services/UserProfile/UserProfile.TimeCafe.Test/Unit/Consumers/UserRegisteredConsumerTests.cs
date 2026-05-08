namespace UserProfile.TimeCafe.Test.Unit.Consumers;

public class UserRegisteredConsumerTests
{
    private readonly Mock<IUserRepositories> _userServiceMock;
    private readonly Mock<ILogger<UserRegisteredConsumer>> _loggerMock;
    private readonly UserRegisteredConsumer _consumer;

    public UserRegisteredConsumerTests()
    {
        _userServiceMock = new Mock<IUserRepositories>();
        _loggerMock = new Mock<ILogger<UserRegisteredConsumer>>();
        _consumer = new UserRegisteredConsumer(_userServiceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Consume_Should_CreateEmptyProfile()
    {
        var userId = Guid.NewGuid();
        var email = "test@example.com";
        var contextMock = CreateConsumeContext(new UserRegisteredEvent
        {
            UserId = userId,
            Email = email
        });

        await _consumer.Consume(contextMock.Object);

        _userServiceMock.Verify(x => x.CreateEmptyAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Consume_Should_Throw_WhenServiceThrows()
    {
        var userId = Guid.NewGuid();
        var contextMock = CreateConsumeContext(new UserRegisteredEvent { UserId = userId });

        _userServiceMock.Setup(x => x.CreateEmptyAsync(userId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("DB error"));

        await _consumer.Invoking(c => c.Consume(contextMock.Object))
            .Should().ThrowAsync<Exception>().WithMessage("DB error");
    }

    private static Mock<ConsumeContext<UserRegisteredEvent>> CreateConsumeContext(UserRegisteredEvent message)
    {
        var mock = new Mock<ConsumeContext<UserRegisteredEvent>>();
        mock.Setup(c => c.Message).Returns(message);
        mock.Setup(c => c.CancellationToken).Returns(CancellationToken.None);
        return mock;
    }
}
