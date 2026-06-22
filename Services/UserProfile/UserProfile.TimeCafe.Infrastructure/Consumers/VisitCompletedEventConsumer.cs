using Microsoft.Extensions.Options;

namespace UserProfile.TimeCafe.Infrastructure.Consumers;

public class VisitCompletedEventConsumer(
    ApplicationDbContext dbContext,
    IOptionsSnapshot<LoyaltyOptions> loyaltyOptions,
    HybridCache cache,
    ILogger<VisitCompletedEventConsumer> logger) : IConsumer<VisitCompletedEvent>
{
    public async Task Consume(ConsumeContext<VisitCompletedEvent> context)
    {
        var message = context.Message;
        try
        {
            logger.LogInformation("Обработка завершения визита {VisitId} для пользователя {UserId}", message.VisitId, message.UserId);

            var profile = await dbContext.Profiles.FirstOrDefaultAsync(p => p.UserId == message.UserId, context.CancellationToken);
            if (profile == null)
            {
                logger.LogWarning("Профиль для пользователя {UserId} не найден", message.UserId);
                return;
            }

            profile.VisitCount++;

            var tiers = loyaltyOptions.Value.Tiers;

            var newDiscount = tiers
                .Where(t => profile.VisitCount >= t.Key)
                .OrderByDescending(t => t.Key)
                .Select(t => t.Value)
                .FirstOrDefault();

            var currentDiscount = profile.PersonalDiscountPercent ?? 0m;

            if (currentDiscount != newDiscount)
            {
                profile.PersonalDiscountPercent = newDiscount;
                logger.LogInformation("Уровень лояльности обновлен для пользователя {UserId}: {Discount}%", message.UserId, newDiscount);

                await context.Publish(new UserDiscountUpdatedEvent
                {
                    UserId = profile.UserId,
                    PersonalDiscountPercent = newDiscount
                }, context.CancellationToken);
            }

            dbContext.Profiles.Update(profile);
            await dbContext.SaveChangesAsync(context.CancellationToken);

            await cache.RemoveAsync(CacheKeys.Profile_ById(profile.UserId), context.CancellationToken);
            await Task.WhenAll(
                cache.RemoveByTagAsync(CacheTags.Profiles, context.CancellationToken).AsTask(),
                cache.RemoveByTagAsync(CacheTags.Profile(profile.UserId), context.CancellationToken).AsTask()
            );
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при обработке завершения визита для пользователя {UserId}", message.UserId);
            throw;
        }
    }
}
