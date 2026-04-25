namespace Venue.TimeCafe.Test.Helpers;

public class VisitBuilder
{
    private Guid _visitId = Guid.NewGuid();
    private Guid _userId = TestData.ExistingVisits.Visit1UserId;
    private Guid _tariffId = TestData.DefaultValues.DefaultTariffId;
    private DateTimeOffset _entryTime = DateTimeOffset.UtcNow;
    private DateTimeOffset? _exitTime;
    private decimal? _calculatedCost;
    private VisitStatus _status = VisitStatus.Active;

    public VisitBuilder WithVisitId(Guid id) { _visitId = id; return this; }
    public VisitBuilder WithUserId(Guid id) { _userId = id; return this; }
    public VisitBuilder WithTariffId(Guid id) { _tariffId = id; return this; }
    public VisitBuilder WithEntryTime(DateTimeOffset time) { _entryTime = time; return this; }
    public VisitBuilder WithExitTime(DateTimeOffset? time) { _exitTime = time; return this; }
    public VisitBuilder WithCalculatedCost(decimal? cost) { _calculatedCost = cost; return this; }
    public VisitBuilder WithStatus(VisitStatus status) { _status = status; return this; }
    public VisitBuilder AsCompleted(decimal cost) { _status = VisitStatus.Completed; _exitTime = DateTimeOffset.UtcNow; _calculatedCost = cost; return this; }

    public Visit Build() => new()
    {
        VisitId = _visitId,
        UserId = _userId,
        TariffId = _tariffId,
        EntryTime = _entryTime,
        ExitTime = _exitTime,
        CalculatedCost = _calculatedCost,
        Status = _status
    };
}

public class CreateVisitCommandBuilder
{
    private Guid _userId = TestData.ExistingVisits.Visit1UserId;
    private Guid _tariffId = TestData.DefaultValues.DefaultTariffId;
    private int? _plannedMinutes;
    private bool _requirePositiveBalance = true;
    private bool _requireEnoughForPlanned;

    public CreateVisitCommandBuilder WithUserId(Guid id) { _userId = id; return this; }
    public CreateVisitCommandBuilder WithTariffId(Guid id) { _tariffId = id; return this; }
    public CreateVisitCommandBuilder WithPlannedMinutes(int? minutes) { _plannedMinutes = minutes; return this; }
    public CreateVisitCommandBuilder WithRequirePositiveBalance(bool v) { _requirePositiveBalance = v; return this; }
    public CreateVisitCommandBuilder WithRequireEnoughForPlanned(bool v) { _requireEnoughForPlanned = v; return this; }

    public CreateVisitCommand Build() => new(_userId, _tariffId, _plannedMinutes, _requirePositiveBalance, _requireEnoughForPlanned);
}

public class PromotionBuilder
{
    private Guid _id = Guid.NewGuid();
    private string _name = TestData.DefaultValues.DefaultPromotionName;
    private string _description = TestData.DefaultValues.DefaultPromotionDescription;
    private decimal? _DiscountPercent = TestData.DefaultValues.DefaultDiscountPercent;
    private DateTimeOffset _validFrom = DateTimeOffset.UtcNow.AddDays(-1);
    private DateTimeOffset _validTo = DateTimeOffset.UtcNow.AddDays(30);
    private bool _isActive = true;

    private PromotionType _type = PromotionType.Global;
    private Guid? _tariffId;

    public PromotionBuilder WithId(Guid id) { _id = id; return this; }
    public PromotionBuilder WithName(string name) { _name = name; return this; }
    public PromotionBuilder WithDescription(string desc) { _description = desc; return this; }
    public PromotionBuilder WithDiscount(decimal? pct) { _DiscountPercent = pct; return this; }
    public PromotionBuilder WithValidRange(DateTimeOffset from, DateTimeOffset to) { _validFrom = from; _validTo = to; return this; }
    public PromotionBuilder AsInactive() { _isActive = false; return this; }
    public PromotionBuilder WithType(PromotionType type) { _type = type; return this; }
    public PromotionBuilder WithTariffId(Guid? tariffId) { _tariffId = tariffId; return this; }

    public Promotion Build() => new()
    {
        PromotionId = _id,
        Name = _name,
        Description = _description,
        DiscountPercent = _DiscountPercent,
        ValidFrom = _validFrom,
        ValidTo = _validTo,
        IsActive = _isActive,
        Type = _type,
        TariffId = _tariffId,
        CreatedAt = DateTimeOffset.UtcNow
    };

    public CreatePromotionCommand BuildCommand() => new(_name, _description, _DiscountPercent, _validFrom, _validTo, _type, _tariffId, _isActive);
}

public class TariffBuilder
{
    private Guid _id = Guid.NewGuid();
    private string _name = TestData.DefaultValues.DefaultTariffName;
    private string? _description;
    private decimal _pricePerMinute = TestData.DefaultValues.DefaultTariffPrice;
    private BillingType _billingType = TestData.DefaultValues.DefaultBillingType;
    private Guid? _themeId;
    private bool _isActive = true;

    public TariffBuilder WithId(Guid id) { _id = id; return this; }
    public TariffBuilder WithName(string name) { _name = name; return this; }
    public TariffBuilder WithDescription(string? desc) { _description = desc; return this; }
    public TariffBuilder WithPrice(decimal price) { _pricePerMinute = price; return this; }
    public TariffBuilder WithBillingType(BillingType type) { _billingType = type; return this; }
    public TariffBuilder WithThemeId(Guid? id) { _themeId = id; return this; }
    public TariffBuilder AsInactive() { _isActive = false; return this; }

    public Tariff Build() => new()
    {
        TariffId = _id,
        Name = _name,
        Description = _description,
        PricePerMinute = _pricePerMinute,
        BillingType = _billingType,
        ThemeId = _themeId,
        IsActive = _isActive,
        CreatedAt = DateTimeOffset.UtcNow,
        LastModified = DateTimeOffset.UtcNow
    };

    public CreateTariffCommand BuildCommand() => new(_name, _description, _pricePerMinute, _billingType, _themeId, _isActive);
}

public class ThemeBuilder
{
    private Guid _id = Guid.NewGuid();
    private string _name = TestData.DefaultValues.DefaultThemeName;
    private string? _emoji = TestData.DefaultValues.DefaultThemeEmoji;
    private string? _colors = TestData.DefaultValues.DefaultThemeColors;

    public ThemeBuilder WithId(Guid id) { _id = id; return this; }
    public ThemeBuilder WithName(string name) { _name = name; return this; }
    public ThemeBuilder WithEmoji(string? emoji) { _emoji = emoji; return this; }
    public ThemeBuilder WithColors(string? colors) { _colors = colors; return this; }

    public Theme Build() => new()
    {
        ThemeId = _id,
        Name = _name,
        Emoji = _emoji,
        Colors = _colors
    };

    public CreateThemeCommand BuildCommand() => new(_name, _emoji, _colors);
}

