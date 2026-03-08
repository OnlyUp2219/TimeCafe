namespace Venue.TimeCafe.API.Extensions;

public static class VenueSeedExtensions
{
    public static async Task SeedFrontendDataAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await SeedThemesAsync(dbContext);
        await SeedTariffsAsync(dbContext);
        await SeedPromotionsAsync(dbContext);
    }

    private static async Task SeedThemesAsync(ApplicationDbContext dbContext)
    {
        var requiredThemes = new[]
        {
            new Theme
            {
                Name = "Базовая",
                Emoji = "☕",
                Colors = "{\"accent\":\"brand\"}"
            },
            new Theme
            {
                Name = "Тихая",
                Emoji = "📚",
                Colors = "{\"accent\":\"green\"}"
            },
            new Theme
            {
                Name = "Ночная",
                Emoji = "🌙",
                Colors = "{\"accent\":\"purple\"}"
            },
            new Theme
            {
                Name = "Промо",
                Emoji = "🎉",
                Colors = "{\"accent\":\"pink\"}"
            }
        };

        var existingNames = await dbContext.Themes
            .Select(x => x.Name)
            .ToListAsync();

        var missing = requiredThemes
            .Where(item => existingNames.All(existing => !string.Equals(existing, item.Name, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        if (missing.Count == 0)
            return;

        await dbContext.Themes.AddRangeAsync(missing);
        await dbContext.SaveChangesAsync();
    }

    private static async Task SeedTariffsAsync(ApplicationDbContext dbContext)
    {
        var themeByName = await dbContext.Themes
            .ToDictionaryAsync(x => x.Name, x => x.ThemeId, StringComparer.OrdinalIgnoreCase);

        Guid? ResolveThemeId(string name) =>
            themeByName.TryGetValue(name, out var value) ? value : null;

        var now = DateTimeOffset.UtcNow;
        var requiredTariffs = new[]
        {
            new Tariff
            {
                Name = "Стандартный",
                Description = "Идеально для большинства визитов — без ограничений и с прозрачным расчётом.",
                BillingType = BillingType.PerMinute,
                PricePerMinute = 3.60m,
                IsActive = true,
                ThemeId = ResolveThemeId("Базовая"),
                CreatedAt = now,
                LastModified = now,
            },
            new Tariff
            {
                Name = "Тихая зона",
                Description = "Спокойная атмосфера и минимальный шум — для работы и учёбы.",
                BillingType = BillingType.PerMinute,
                PricePerMinute = 3.00m,
                IsActive = true,
                ThemeId = ResolveThemeId("Тихая"),
                CreatedAt = now,
                LastModified = now,
            },
            new Tariff
            {
                Name = "Ночной",
                Description = "Для поздних визитов: мягкий свет, меньше людей и приятный темп.",
                BillingType = BillingType.Hourly,
                PricePerMinute = 180m,
                IsActive = true,
                ThemeId = ResolveThemeId("Ночная"),
                CreatedAt = now,
                LastModified = now,
            },
            new Tariff
            {
                Name = "Промо",
                Description = "Тариф для акций и промокодов.",
                BillingType = BillingType.Hourly,
                PricePerMinute = 666m,
                IsActive = true,
                ThemeId = ResolveThemeId("Промо"),
                CreatedAt = now,
                LastModified = now,
            },
            new Tariff
            {
                Name = "Ранний",
                Description = "Лёгкий тариф для утренних визитов.",
                BillingType = BillingType.PerMinute,
                PricePerMinute = 2.40m,
                IsActive = true,
                ThemeId = ResolveThemeId("Базовая"),
                CreatedAt = now,
                LastModified = now,
            },
        };

        var existingNames = await dbContext.Tariffs
            .Select(x => x.Name)
            .ToListAsync();

        var missing = requiredTariffs
            .Where(item => existingNames.All(existing => !string.Equals(existing, item.Name, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        if (missing.Count == 0)
            return;

        await dbContext.Tariffs.AddRangeAsync(missing);
        await dbContext.SaveChangesAsync();
    }

    private static async Task SeedPromotionsAsync(ApplicationDbContext dbContext)
    {
        var now = DateTimeOffset.UtcNow;
        var requiredPromotions = new[]
        {
            new Promotion
            {
                Name = "Welcome 10%",
                Description = "Скидка для новых гостей на первый визит.",
                DiscountPercent = 10,
                ValidFrom = now.Date,
                ValidTo = now.Date.AddMonths(6),
                IsActive = true,
                CreatedAt = now,
            },
            new Promotion
            {
                Name = "Student 15%",
                Description = "Скидка для студентов в дневное время.",
                DiscountPercent = 15,
                ValidFrom = now.Date,
                ValidTo = now.Date.AddMonths(12),
                IsActive = true,
                CreatedAt = now,
            }
        };

        var existingNames = await dbContext.Promotions
            .Select(x => x.Name)
            .ToListAsync();

        var missing = requiredPromotions
            .Where(item => existingNames.All(existing => !string.Equals(existing, item.Name, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        if (missing.Count == 0)
            return;

        await dbContext.Promotions.AddRangeAsync(missing);
        await dbContext.SaveChangesAsync();
    }
}
