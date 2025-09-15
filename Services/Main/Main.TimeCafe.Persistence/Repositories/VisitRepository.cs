namespace TimeCafe.Persistence.Repositories;

public class VisitRepository : IVisitRepository
{
    private readonly TimeCafeContext _context;
    private readonly IBillingTypeRepository _billingTypeRepository;
    private readonly IFinancialRepository _financialRepository;
    private readonly IVisitRepository _visitRepository;
    private readonly IDistributedCache _cache;
    private readonly ILogger<VisitRepository> _logger;
    public VisitRepository(
        TimeCafeContext context,
        IBillingTypeRepository billingTypeRepository,
        IFinancialRepository financialRepository,
        IVisitRepository visitRepository,
        IDistributedCache cache,
        ILogger<VisitRepository> logger)
    {
        _context = context;
        _billingTypeRepository = billingTypeRepository;
        _financialRepository = financialRepository;
        _visitRepository = visitRepository;
        _cache = cache;
        _logger = logger;
    }

    public async Task<IEnumerable<Visit>> GetActiveVisitsAsync()
    {
        var cached = await CacheHelper.GetAsync<IEnumerable<Visit>>(
            _cache,
            _logger,
            CacheKeys.Visit_All);
        if (cached != null)
            return cached;

        var entity = await _context.Visits
           .Include(v => v.Client)
           .Include(v => v.Tariff)
           .Include(v => v.BillingType)
           .Where(v => v.ExitTime == null)
           .OrderByDescending(v => v.EntryTime)
           .ToListAsync();

        await CacheHelper.SetAsync(
            _cache,
            _logger,
            CacheKeys.Visit_All,
            entity);

        return entity;
    }

    public async Task<Visit> GetActiveVisitByClientAsync(int clientId)
    {
        var cached = await CacheHelper.GetAsync<Visit>(
            _cache,
            _logger,
            CacheKeys.Visit_ByCliendId(clientId));
        if (cached != null)
            return cached;

        var entity = await _context.Visits
            .Include(v => v.Tariff)
            .Include(v => v.BillingType)
            .FirstOrDefaultAsync(v => v.ClientId == clientId && v.ExitTime == null);

        await CacheHelper.SetAsync(
            _cache,
            _logger,
            CacheKeys.Visit_ByCliendId(clientId),
            entity);

        return entity;
    }

    public async Task<bool> IsClientActiveAsync(int clientId)
    {
        var client = await _context.Clients
            .Include(c => c.Status)
            .FirstOrDefaultAsync(c => c.ClientId == clientId);

        return client?.Status?.StatusName == "Активный";
    }

    public async Task<bool> IsClientAlreadyEnteredAsync(int clientId)
    {
        return await _context.Visits
            .AnyAsync(v => v.ClientId == clientId && v.ExitTime == null);
    }


    public async Task<Visit> EnterClientAsync(int clientId, int tariffId, int minimumEntryMinutes)
    {
        if (!await _visitRepository.IsClientActiveAsync(clientId))
            throw new InvalidOperationException("Клиент не имеет активного статуса");

        if (await _visitRepository.IsClientAlreadyEnteredAsync(clientId))
            throw new InvalidOperationException("Ошибка. Вход уже осуществлен");

        var tariff = await _context.Tariffs
            .Include(t => t.BillingType)
            .FirstOrDefaultAsync(t => t.TariffId == tariffId);

        if (tariff == null)
            throw new KeyNotFoundException($"Тариф с ID {tariffId} не найден");

        if (tariff.BillingType == null)
            throw new InvalidOperationException($"У тарифа {tariff.TariffName} не указан тип тарификации");

        var minRequiredAmount = CalculateMinimumRequiredAmount(tariff, minimumEntryMinutes);
        if (!await _financialRepository.HasSufficientBalanceAsync(clientId, minRequiredAmount))
        {
            var currentBalance = await _financialRepository.GetClientBalanceAsync(clientId);
            throw new InvalidOperationException($"Недостаточно средств для входа. Требуется минимум {minRequiredAmount:C} (стоимость {minimumEntryMinutes} минут), доступно {currentBalance:C}");
        }

        var visit = new Visit
        {
            ClientId = clientId,
            TariffId = tariffId,
            BillingTypeId = tariff.BillingTypeId,
            EntryTime = DateTime.Now,
            ExitTime = null,
            VisitCost = null
        };

        _context.Visits.Add(visit);
        try
        {
            await _context.SaveChangesAsync();

            await CacheHelper.RemoveKeysAsync(
                 _cache,
                 _logger,
                 CacheKeys.Visit_All,
                 CacheKeys.Visit_ByCliendId(clientId));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка сохранения Visit: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Inner Exception: {ex.InnerException?.Message}");
            throw new InvalidOperationException($"Ошибка создания посещения: {ex.Message}", ex);
        }

        return visit;
    }

    public async Task<Visit> ExitClientAsync(int visitId)
    {
        var visit = await _context.Visits
            .Include(v => v.Tariff)
            .Include(v => v.BillingType)
            .Include(v => v.Client)
            .FirstOrDefaultAsync(v => v.VisitId == visitId);

        if (visit == null)
            throw new KeyNotFoundException($"Посещение с ID {visitId} не найдено");

        if (visit.ExitTime.HasValue)
            throw new InvalidOperationException("Ошибка. Выход уже осуществлен");

        visit.ExitTime = DateTime.Now;
        visit.VisitCost = await CalculateVisitCostAsync(visit);

        await _financialRepository.DeductAsync(visit.ClientId.Value, visit.VisitCost.Value, visit.VisitId, "Оплата посещения");

        await _context.SaveChangesAsync();

        await CacheHelper.RemoveKeysAsync(
                _cache,
                _logger,
                CacheKeys.Visit_All,
                CacheKeys.Visit_ByCliendId(visit.ClientId ?? 0));

        return visit;
    }


    public async Task<decimal> CalculateVisitCostAsync(Visit visit)
    {
        if (visit.Tariff == null)
            return 0;

        var duration = GetVisitDuration(visit);

        if (!visit.BillingTypeId.HasValue)
            return 0;

        var billingType = await _billingTypeRepository.GetBillingTypeByIdAsync(visit.BillingTypeId.Value);

        if (billingType == null)
            return 0;

        switch (billingType.BillingTypeId)
        {
            case 1:
                var hours = Math.Ceiling(duration.TotalHours);
                return visit.Tariff.Price * (decimal)hours;

            case 2:
                var minutes = duration.TotalMinutes;
                return visit.Tariff.Price * (decimal)minutes;

            default:
                return 0;
        }

    }

    public TimeSpan GetVisitDuration(Visit visit)
    {
        var endTime = visit.ExitTime ?? DateTime.Now;
        return endTime - visit.EntryTime;
    }

    private decimal CalculateMinimumRequiredAmount(Tariff tariff, int minMinutes)
    {
        if (tariff.BillingType == null)
            return 0;

        switch (tariff.BillingType.BillingTypeId)
        {
            case 1:
                return tariff.Price * minMinutes / 60m;
            case 2:
                return tariff.Price * minMinutes;
            default:
                return 0;
        }

    }


    #region Todo
    // TODO: Автоматический выход всех посетителей при закрытии заведения
    // Этот метод будет вызываться при закрытии приложения или изменении рабочих часов
    /*
    public async Task ExitAllVisitorsAsync(string reason = "Автоматический выход при закрытии заведения")
    {
        var activeVisits = await _context.Visits
            .Include(v => v.Client)
            .Include(v => v.Tariff)
            .Include(v => v.BillingType)
            .Where(v => v.ExitTime == null)
            .ToListAsync();

        foreach (var visit in activeVisits)
        {
            visit.ExitTime = DateTime.Now;
            visit.VisitCost = await CalculateVisitCostAsync(visit);
            
            // TODO: Логирование автоматического выхода
            // await _logger.LogAsync($"Автоматический выход клиента {visit.Client?.FirstName} {visit.Client?.LastName} по причине: {reason}");
        }

        await _context.SaveChangesAsync();
    }
    */
    #endregion
}