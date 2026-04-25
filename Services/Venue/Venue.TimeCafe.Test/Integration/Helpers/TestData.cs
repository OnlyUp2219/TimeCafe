namespace Venue.TimeCafe.Test.Integration.Helpers;

/// <summary>
/// Централизованное хранилище тестовых данных для интеграционных тестов.
/// Изменяйте только этот файл при необходимости обновления тестовых данных.
/// </summary>
public static class TestData
{
    /// <summary>
    /// Основные акции для тестирования (используются в GET-тестах)
    /// </summary>
    public static class ExistingPromotions
    {
        public static readonly Guid Promotion1Id = Guid.Parse("11111111-1111-1111-1111-111111111111");
        public static readonly string Promotion1Name = "Летняя акция";
        public static readonly string Promotion1Description = "Скидка 15% на все напитки";
        public static readonly decimal Promotion1DiscountPercent = 15m;

        public static readonly Guid Promotion2Id = Guid.Parse("22222222-2222-2222-2222-222222222222");
        public static readonly string Promotion2Name = "Праздничная скидка";
        public static readonly string Promotion2Description = "Скидка 25% на выходные";
        public static readonly decimal Promotion2DiscountPercent = 25m;

        public static readonly Guid Promotion3Id = Guid.Parse("33333333-3333-3333-3333-333333333333");
        public static readonly string Promotion3Name = "Постоянная акция";
        public static readonly string Promotion3Description = "Скидка 10% постоянно";
        public static readonly decimal Promotion3DiscountPercent = 10m;
    }

    /// <summary>
    /// Новые акции для создания (используются в CREATE-тестах)
    /// </summary>
    public static class NewPromotions
    {
        public static readonly string NewPromotion1Name = "Новая весенняя акция";
        public static readonly string NewPromotion1Description = "Специальное предложение";
        public static readonly decimal NewPromotion1DiscountPercent = 20m;

        public static readonly string NewPromotion2Name = "Тестовая акция";
        public static readonly string NewPromotion2Description = "Тестовое описание";
        public static readonly decimal NewPromotion2DiscountPercent = 5m;
    }

    /// <summary>
    /// Данные для обновления акций
    /// </summary>
    public static class UpdateData
    {
        public static readonly string UpdatedPromotionName = "Обновленное название акции";
        public static readonly string UpdatedPromotionDescription = "Обновленное описание";
        public static readonly decimal UpdatedDiscountPercent = 30m;
    }

    /// <summary>
    /// Несуществующие ID для тестирования ошибок
    /// </summary>
    public static class NonExistingIds
    {
        public static readonly Guid NonExistingPromotionId = Guid.Parse("99999999-9999-9999-9999-999999999999");
        public static readonly string NonExistingPromotionIdString = "99999999-9999-9999-9999-999999999999";
        public static readonly Guid NonExistingTariffId = Guid.Parse("99999999-9999-9999-9999-999999999998");
        public static readonly string NonExistingTariffIdString = "99999999-9999-9999-9999-999999999998";
        public static readonly Guid NonExistingThemeId = Guid.Parse("99999999-9999-9999-9999-999999999997");
        public static readonly Guid NonExistingVisitId = Guid.Parse("99999999-9999-9999-9999-999999999996");
        public static readonly string NonExistingVisitIdString = "99999999-9999-9999-9999-999999999996";
        public static readonly Guid NonExistingUserId = Guid.Parse("99999999-9999-9999-9999-999999999995");
        public static readonly string NonExistingUserIdString = "99999999-9999-9999-9999-999999999995";
    }

    /// <summary>
    /// Некорректные ID для валидации
    /// </summary>
    public static class InvalidIds
    {
        public static readonly Guid EmptyGuid = Guid.Empty;
        public static readonly string EmptyGuidString = "00000000-0000-0000-0000-000000000000";
        public static readonly string NotAGuid = "not-a-guid";
        public static readonly string InvalidGuidFormat = "123e4567-e89b-12d3-a456-42661417400g";
        public static readonly string EmptyString = "";
    }

    /// <summary>
    /// Основные тарифы для тестирования
    /// </summary>
    public static class ExistingTariffs
    {
        public static readonly Guid Tariff1Id = Guid.Parse("11111111-aaaa-1111-1111-111111111111");
        public static readonly string Tariff1Name = "Эконом";
        public static readonly decimal Tariff1PricePerMinute = 1.0m;
        public static readonly BillingType Tariff1BillingType = BillingType.PerMinute;

        public static readonly Guid Tariff2Id = Guid.Parse("22222222-bbbb-2222-2222-222222222222");
        public static readonly string Tariff2Name = "Стандарт";
        public static readonly decimal Tariff2PricePerMinute = 2.0m;
        public static readonly BillingType Tariff2BillingType = BillingType.PerMinute;

        public static readonly Guid Tariff3Id = Guid.Parse("33333333-cccc-3333-3333-333333333333");
        public static readonly string Tariff3IdString = "33333333-cccc-3333-3333-333333333333";
        public static readonly string Tariff3Name = "Премиум";
        public static readonly decimal Tariff3PricePerMinute = 3.0m;
        public static readonly BillingType Tariff3BillingType = BillingType.PerMinute;
    }

    /// <summary>
    /// Новые тарифы для создания
    /// </summary>
    public static class NewTariffs
    {
        public static readonly Guid NewTariff1Id = Guid.Parse("44444444-aaaa-4444-4444-444444444444");
        public static readonly string NewTariff1IdString = "44444444-aaaa-4444-4444-444444444444";
        public static readonly string NewTariff1Name = "Новый тариф";
        public static readonly decimal NewTariff1Price = 2.5m;
        public static readonly BillingType NewTariff1BillingType = BillingType.PerMinute;

        public static readonly Guid NewTariff2Id = Guid.Parse("55555555-bbbb-5555-5555-555555555555");
        public static readonly string NewTariff2IdString = "55555555-bbbb-5555-5555-555555555555";
        public static readonly string NewTariff2Name = "Специальный тариф";
        public static readonly decimal NewTariff2Price = 1.5m;
        public static readonly BillingType NewTariff2BillingType = BillingType.Hourly;
    }

    /// <summary>
    /// Основные темы для тестирования
    /// </summary>
    public static class ExistingThemes
    {
        public static readonly Guid Theme1Id = Guid.Parse("a1111111-1111-1111-1111-111111111111");
        public static readonly string Theme1Name = "Темная тема";
        public static readonly string Theme1Emoji = "🌙";
        public static readonly string Theme1Colors = "{\"primary\":\"#1a1a1a\",\"secondary\":\"#ffffff\"}";

        public static readonly Guid Theme2Id = Guid.Parse("a2222222-2222-2222-2222-222222222222");
        public static readonly string Theme2Name = "Светлая тема";
        public static readonly string Theme2Emoji = "☀️";
        public static readonly string Theme2Colors = "{\"primary\":\"#ffffff\",\"secondary\":\"#000000\"}";

        public static readonly Guid Theme3Id = Guid.Parse("a3333333-3333-3333-3333-333333333333");
        public static readonly string Theme3Name = "Синяя тема";
        public static readonly string Theme3Emoji = "🔵";
        public static readonly string Theme3Colors = "{\"primary\":\"#0066cc\",\"secondary\":\"#ffffff\"}";
    }

    /// <summary>
    /// Новые темы для создания
    /// </summary>
    public static class NewThemes
    {
        public static readonly string NewTheme1Name = "Зеленая тема";
        public static readonly string NewTheme1Emoji = "💚";
        public static readonly string NewTheme1Colors = "{\"primary\":\"#00aa00\",\"secondary\":\"#ffffff\"}";

        public static readonly string NewTheme2Name = "Красная тема";
        public static readonly string NewTheme2Emoji = "❤️";
        public static readonly string NewTheme2Colors = "{\"primary\":\"#ff0000\",\"secondary\":\"#ffffff\"}";
    }

    /// <summary>
    /// Основные данные посещений для тестирования
    /// </summary>
    public static class ExistingVisits
    {
        public static readonly Guid Visit1UserId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        public static readonly Guid Visit2UserId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        public static readonly Guid Visit3UserId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    }

    /// <summary>
    /// Новые посещения для создания
    /// </summary>
    public static class NewVisits
    {
        public static readonly Guid NewVisit1UserId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        public static readonly Guid NewVisit2UserId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
    }

    /// <summary>
    /// Данные для обновления посещений
    /// </summary>
    public static class VisitUpdateData
    {
        public static readonly decimal UpdatedCalculatedCost = 300m;
    }

    /// <summary>
    /// Дефолтные значения для тестов
    /// </summary>
    public static class DefaultValues
    {
        public static readonly string DefaultPromotionName = "Test Promotion";
        public static readonly string DefaultPromotionDescription = "Test Description";
        public static readonly decimal DefaultDiscountPercent = 10m;

        public static readonly string DefaultTariffName = "Test Tariff";
        public static readonly decimal DefaultTariffPrice = 100m;
        public static readonly BillingType DefaultBillingType = BillingType.PerMinute;
        public static readonly Guid DefaultTariffId = Guid.Parse("66666666-6666-6666-6666-666666666666");
        public static readonly string DefaultTariffIdString = "66666666-6666-6666-6666-666666666666";

        public static readonly string DefaultThemeName = "Test Theme";
        public static readonly string DefaultThemeEmoji = "🎨";
        public static readonly string DefaultThemeColors = "{\"primary\":\"#000000\",\"secondary\":\"#FFFFFF\"}";

        public static readonly Guid DefaultUserId = Guid.Parse("12345678-1234-1234-1234-123456789012");

        public static readonly int DefaultPageSize = 10;
        public static readonly int FirstPage = 1;
        public static readonly int SecondPage = 2;
        public static readonly int InvalidPage = 0;
    }

    /// <summary>
    /// Граничные значения для валидации
    /// </summary>
    public static class ValidationBoundaries
    {
        public static readonly string EmptyString = "";
        public static readonly string? NullString = null;
        public static readonly string VeryLongString = new('a', 1000);
        public static readonly decimal ZeroDiscount = 0m;
        public static readonly decimal NegativeDiscount = -10m;
        public static readonly decimal MaxDiscount = 100m;
        public static readonly decimal OverMaxDiscount = 101m;
        public static readonly decimal ZeroPrice = 0m;
        public static readonly decimal NegativePrice = -10m;
    }

    /// <summary>
    /// Временные данные для тестов
    /// </summary>
    public static class DateTimeData
    {
        public static DateTimeOffset GetValidFromDate() => DateTimeOffset.UtcNow;
        public static DateTimeOffset GetValidToDate() => DateTimeOffset.UtcNow.AddDays(30);
        public static DateTimeOffset GetPastDate() => DateTimeOffset.UtcNow.AddDays(-7);
        public static DateTimeOffset GetFutureDate() => DateTimeOffset.UtcNow.AddDays(60);
    }
}








