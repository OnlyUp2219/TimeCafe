namespace Venue.TimeCafe.Infrastructure.Consumers;

public class UserDiscountUpdatedEventConsumer(
    IUnitOfWork uow,
    IPublisher publisher,
    ILogger<UserDiscountUpdatedEventConsumer> logger) : IConsumer<UserDiscountUpdatedEvent>
{
    public async Task Consume(ConsumeContext<UserDiscountUpdatedEvent> context)
    {
        var message = context.Message;
        logger.LogInformation("Обновление скидки для пользователя {UserId} до {Discount}%", message.UserId, message.PersonalDiscountPercent);

        var userLoyalty = await uow.UserLoyalties.GetByUserIdAsync(message.UserId, context.CancellationToken);
        if (userLoyalty == null)
        {
            userLoyalty = new UserLoyalty(message.UserId) { PersonalDiscountPercent = message.PersonalDiscountPercent };
            await uow.UserLoyalties.CreateAsync(userLoyalty, context.CancellationToken);
        }
        else
        {
            userLoyalty.PersonalDiscountPercent = message.PersonalDiscountPercent;
            userLoyalty.LastUpdated = DateTimeOffset.UtcNow;
            await uow.UserLoyalties.UpdateAsync(userLoyalty, context.CancellationToken);
        }

        await uow.SaveChangesAsync(context.CancellationToken);
        await publisher.Publish(new UserLoyaltyChangedEvent(message.UserId), context.CancellationToken);
    }
}

