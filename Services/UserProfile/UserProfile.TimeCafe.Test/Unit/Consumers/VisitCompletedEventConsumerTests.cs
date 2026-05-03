using BuildingBlocks.Events;
using MassTransit;
using Microsoft.Extensions.Options;
using UserProfile.TimeCafe.Application.Options;
using UserProfile.TimeCafe.Infrastructure.Consumers;

namespace UserProfile.TimeCafe.Test.Unit.Consumers;

public class VisitCompletedEventConsumerTests
{
    private readonly ApplicationDbContext _dbContext;
    private readonly VisitCompletedEventConsumer _consumer;

    public VisitCompletedEventConsumerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ApplicationDbContext(options);

        var loyaltyOptions = Microsoft.Extensions.Options.Options.Create(new LoyaltyOptions());
        var snapshotMock = new Mock<IOptionsSnapshot<LoyaltyOptions>>();
        snapshotMock.Setup(x => x.Value).Returns(loyaltyOptions.Value);

        var logger = new Mock<ILogger<VisitCompletedEventConsumer>>();
        _consumer = new VisitCompletedEventConsumer(_dbContext, snapshotMock.Object, logger.Object);
    }

    [Fact]
    public async Task Consume_Should_IncrementVisitCount()
    {
        var userId = Guid.NewGuid();
        _dbContext.Profiles.Add(new Profile
        {
            UserId = userId,
            FirstName = "Test",
            LastName = "User",
            ProfileStatus = ProfileStatus.Completed,
            VisitCount = 0
        });
        await _dbContext.SaveChangesAsync();

        var contextMock = CreateConsumeContext(new VisitCompletedEvent
        {
            VisitId = Guid.NewGuid(),
            UserId = userId,
            Amount = 100,
            CompletedAt = DateTimeOffset.UtcNow
        });

        await _consumer.Consume(contextMock.Object);

        var profile = await _dbContext.Profiles.FirstAsync(p => p.UserId == userId);
        profile.VisitCount.Should().Be(1);
    }

    [Fact]
    public async Task Consume_Should_SetDiscount_WhenTierReached()
    {
        var userId = Guid.NewGuid();
        _dbContext.Profiles.Add(new Profile
        {
            UserId = userId,
            FirstName = "Test",
            LastName = "User",
            ProfileStatus = ProfileStatus.Completed,
            VisitCount = 4
        });
        await _dbContext.SaveChangesAsync();

        var contextMock = CreateConsumeContext(new VisitCompletedEvent
        {
            VisitId = Guid.NewGuid(),
            UserId = userId,
            Amount = 100,
            CompletedAt = DateTimeOffset.UtcNow
        });

        await _consumer.Consume(contextMock.Object);

        var profile = await _dbContext.Profiles.FirstAsync(p => p.UserId == userId);
        profile.VisitCount.Should().Be(5);
        profile.PersonalDiscountPercent.Should().Be(5m);
    }

    [Fact]
    public async Task Consume_Should_NotChangeDiscount_WhenBetweenTiers()
    {
        var userId = Guid.NewGuid();
        _dbContext.Profiles.Add(new Profile
        {
            UserId = userId,
            FirstName = "Test",
            LastName = "User",
            ProfileStatus = ProfileStatus.Completed,
            VisitCount = 6,
            PersonalDiscountPercent = 5m
        });
        await _dbContext.SaveChangesAsync();

        var contextMock = CreateConsumeContext(new VisitCompletedEvent
        {
            VisitId = Guid.NewGuid(),
            UserId = userId,
            Amount = 100,
            CompletedAt = DateTimeOffset.UtcNow
        });

        await _consumer.Consume(contextMock.Object);

        var profile = await _dbContext.Profiles.FirstAsync(p => p.UserId == userId);
        profile.VisitCount.Should().Be(7);
        profile.PersonalDiscountPercent.Should().Be(5m);
    }

    [Fact]
    public async Task Consume_Should_SkipIfProfileNotFound()
    {
        var contextMock = CreateConsumeContext(new VisitCompletedEvent
        {
            VisitId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Amount = 100,
            CompletedAt = DateTimeOffset.UtcNow
        });

        await _consumer.Consume(contextMock.Object);

        _dbContext.Profiles.Should().BeEmpty();
    }

    [Fact]
    public async Task Consume_Should_PublishEvent_WhenDiscountChanges()
    {
        var userId = Guid.NewGuid();
        _dbContext.Profiles.Add(new Profile
        {
            UserId = userId,
            FirstName = "Test",
            LastName = "User",
            ProfileStatus = ProfileStatus.Completed,
            VisitCount = 9
        });
        await _dbContext.SaveChangesAsync();

        var contextMock = CreateConsumeContext(new VisitCompletedEvent
        {
            VisitId = Guid.NewGuid(),
            UserId = userId,
            Amount = 100,
            CompletedAt = DateTimeOffset.UtcNow
        });

        await _consumer.Consume(contextMock.Object);

        contextMock.Verify(c => c.Publish(
            It.Is<UserDiscountUpdatedEvent>(e => e.UserId == userId && e.PersonalDiscountPercent == 10m),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    private static Mock<ConsumeContext<VisitCompletedEvent>> CreateConsumeContext(VisitCompletedEvent message)
    {
        var mock = new Mock<ConsumeContext<VisitCompletedEvent>>();
        mock.Setup(c => c.Message).Returns(message);
        mock.Setup(c => c.CancellationToken).Returns(CancellationToken.None);
        return mock;
    }
}
