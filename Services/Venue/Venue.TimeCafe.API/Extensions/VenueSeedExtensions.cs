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
        await SeedResourceGroupsAsync(dbContext);
        await SeedResourcesAsync(dbContext);
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
                Summary = "Классический тариф для отдыха и общения.",
                Features = ["Бесплатный чай и кофе", "Настольные игры", "Wi-Fi"],
                AudienceTags = ["Популярный", "Для компаний"],
                MinSessionMinutes = 30,
                RoundingRule = "5min",
                IsRecommended = true,
                SortOrder = 1,
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
                Summary = "Идеально для работы и учебы в тишине.",
                Features = ["Тихая атмосфера", "Розетки у каждого стола", "Чай включен"],
                AudienceTags = ["Для работы", "Для учебы"],
                MinSessionMinutes = 60,
                RoundingRule = "15min",
                SortOrder = 2,
                CreatedAt = now,
                LastModified = now,
            },
            new Tariff
            {
                Name = "Ночной",
                Description = "Для поздних визитов: мягкий свет, меньше людей и приятный темп.",
                BillingType = BillingType.Hourly,
                PricePerMinute = 3.00m, // 180 rub per hour
                IsActive = true,
                ThemeId = ResolveThemeId("Ночная"),
                Summary = "Для тех, кто любит ночную атмосферу.",
                Features = ["Мягкий свет", "Приятная музыка", "Настолки"],
                AudienceTags = ["Ночь", "Атмосфера"],
                MinSessionMinutes = 120,
                RoundingRule = "60min",
                MaxGuests = 4,
                SortOrder = 3,
                CreatedAt = now,
                LastModified = now,
            },
            new Tariff
            {
                Name = "Промо",
                Description = "Тариф для акций и промокодов.",
                BillingType = BillingType.Hourly,
                PricePerMinute = 4.00m, // 240 rub per hour
                IsActive = true,
                ThemeId = ResolveThemeId("Промо"),
                Summary = "Специальный тариф для мероприятий.",
                Features = ["Особые условия"],
                AudienceTags = ["Акции"],
                MinSessionMinutes = 60,
                RoundingRule = "60min",
                SortOrder = 4,
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
                Summary = "Скидка для жаворонков.",
                Features = ["Утренний кофе"],
                AudienceTags = ["Утро"],
                MinSessionMinutes = 15,
                RoundingRule = "5min",
                SortOrder = 5,
                CreatedAt = now,
                LastModified = now,
            },
        };

        foreach (var required in requiredTariffs)
        {
            var existing = await dbContext.Tariffs.FirstOrDefaultAsync(x => x.Name == required.Name);
            if (existing != null)
            {
                existing.Description = required.Description;
                existing.BillingType = required.BillingType;
                existing.PricePerMinute = required.PricePerMinute;
                existing.ThemeId = required.ThemeId;
                existing.Summary = required.Summary;
                existing.Features = required.Features;
                existing.AudienceTags = required.AudienceTags;
                existing.MinSessionMinutes = required.MinSessionMinutes;
                existing.RoundingRule = required.RoundingRule;
                existing.MaxGuests = required.MaxGuests;
                existing.IsRecommended = required.IsRecommended;
                existing.SortOrder = required.SortOrder;
                existing.LastModified = now;
            }
            else
            {
                await dbContext.Tariffs.AddAsync(required);
            }
        }

        await dbContext.SaveChangesAsync();
    }

    private static async Task SeedPromotionsAsync(ApplicationDbContext dbContext)
    {
        var now = DateTimeOffset.UtcNow;
        var todayUtc = new DateTimeOffset(now.Date, TimeSpan.Zero);

        var requiredPromotions = new[]
        {
            new Promotion
            {
                Name = "Welcome 10%",
                Description = "Скидка для новых гостей на первый визит.",
                DiscountPercent = 10,
                ValidFrom = todayUtc,
                ValidTo = todayUtc.AddMonths(6),
                IsActive = true,
                Type = PromotionType.Global,
                CreatedAt = now,
            },
            new Promotion
            {
                Name = "Student 15%",
                Description = "Скидка для студентов в дневное время.",
                DiscountPercent = 15,
                ValidFrom = todayUtc,
                ValidTo = todayUtc.AddMonths(12),
                IsActive = false,
                Type = PromotionType.Draft,
                TariffId = null,
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

    private static async Task SeedResourceGroupsAsync(ApplicationDbContext dbContext)
    {
        var requiredGroups = new[]
        {
            new ResourceGroup
            {
                Name = "Основной зал",
                Description = "Общая уютная зона для настольных игр и общения",
                Capacity = 40,
                IsActive = true
            },
            new ResourceGroup
            {
                Name = "Игровая зона",
                Description = "Комнаты с игровыми консолями PlayStation 5 и Xbox",
                Capacity = 10,
                IsActive = true
            },
            new ResourceGroup
            {
                Name = "Коворкинг",
                Description = "Тихая рабочая зона для фрилансеров и учебы",
                Capacity = 12,
                IsActive = true
            }
        };

        foreach (var required in requiredGroups)
        {
            var existing = await dbContext.ResourceGroups.FirstOrDefaultAsync(x => x.Name == required.Name);
            if (existing != null)
            {
                existing.Description = required.Description;
                existing.Capacity = required.Capacity;
                existing.IsActive = required.IsActive;
            }
            else
            {
                await dbContext.ResourceGroups.AddAsync(required);
            }
        }

        await dbContext.SaveChangesAsync();
    }

    private static async Task SeedResourcesAsync(ApplicationDbContext dbContext)
    {
        var groups = await dbContext.ResourceGroups.ToDictionaryAsync(x => x.Name, x => x.ResourceGroupId, StringComparer.OrdinalIgnoreCase);

        Guid? ResolveGroupId(string name) =>
            groups.TryGetValue(name, out var value) ? value : null;

        var mainGroupId = ResolveGroupId("Основной зал");
        var gameGroupId = ResolveGroupId("Игровая зона");
        var coworkGroupId = ResolveGroupId("Коворкинг");

        if (mainGroupId == null || gameGroupId == null || coworkGroupId == null)
            return;

        var requiredResources = new[]
        {
            new Resource { Name = "Стол №1 (У окна)", Capacity = 4, ResourceGroupId = mainGroupId.Value, IsActive = true },
            new Resource { Name = "Стол №2 (Диван)", Capacity = 6, ResourceGroupId = mainGroupId.Value, IsActive = true },
            new Resource { Name = "Стол №3 (Большой)", Capacity = 8, ResourceGroupId = mainGroupId.Value, IsActive = true },
            new Resource { Name = "Стол №4 (Семейный)", Capacity = 10, ResourceGroupId = mainGroupId.Value, IsActive = true },
            new Resource { Name = "VIP комната (PS5 Pro)", Capacity = 4, ResourceGroupId = gameGroupId.Value, IsActive = true },
            new Resource { Name = "VIP комната (Xbox Series)", Capacity = 4, ResourceGroupId = gameGroupId.Value, IsActive = true },
            new Resource { Name = "Место №1 (Коворкинг)", Capacity = 1, ResourceGroupId = coworkGroupId.Value, IsActive = true },
            new Resource { Name = "Место №2 (Коворкинг)", Capacity = 1, ResourceGroupId = coworkGroupId.Value, IsActive = true },
            new Resource { Name = "Место №3 (Коворкинг)", Capacity = 1, ResourceGroupId = coworkGroupId.Value, IsActive = true }
        };

        foreach (var required in requiredResources)
        {
            var existing = await dbContext.Resources.FirstOrDefaultAsync(x => x.Name == required.Name && x.ResourceGroupId == required.ResourceGroupId);
            if (existing != null)
            {
                existing.Capacity = required.Capacity;
                existing.IsActive = required.IsActive;
            }
            else
            {
                await dbContext.Resources.AddAsync(required);
            }
        }

        await dbContext.SaveChangesAsync();
    }
}

