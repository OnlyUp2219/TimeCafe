namespace Venue.TimeCafe.Infrastructure.Consumers;

public class UserDiscountUpdatedEventConsumer(ApplicationDbContext dbContext, ILogger<UserDiscountUpdatedEventConsumer> logger) : IConsumer<UserDiscountUpdatedEvent>
{
    public async Task Consume(ConsumeContext<UserDiscountUpdatedEvent> context)
    {
        var message = context.Message;
        logger.LogInformation("Обновление скидки для пользователя {UserId} до {Discount}%", message.UserId, message.PersonalDiscountPercent);

        var userLoyalty = await dbContext.UserLoyalties.FirstOrDefaultAsync(x => x.UserId == message.UserId, context.CancellationToken);
        if (userLoyalty == null)
        {
            userLoyalty = new UserLoyalty(message.UserId) { PersonalDiscountPercent = message.PersonalDiscountPercent };
            dbContext.UserLoyalties.Add(userLoyalty);
        }
        else
        {
            userLoyalty.PersonalDiscountPercent = message.PersonalDiscountPercent;
            userLoyalty.LastUpdated = DateTimeOffset.UtcNow;
            dbContext.UserLoyalties.Update(userLoyalty);
        }

        await dbContext.SaveChangesAsync(context.CancellationToken);
    }
}

