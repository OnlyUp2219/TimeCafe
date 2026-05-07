namespace Venue.TimeCafe.Infrastructure.Data;

public class UnitOfWork(ApplicationDbContext context, HybridCache cache) : IUnitOfWork
{
    private readonly ApplicationDbContext _context = context;
    private readonly HybridCache _cache = cache;

    public ITariffRepository Tariffs => field ??= new TariffRepository(_context, _cache);
    public IPromotionRepository Promotions => field ??= new PromotionRepository(_context, _cache);
    public IThemeRepository Themes => field ??= new ThemeRepository(_context, _cache);
    public IVisitRepository Visits => field ??= new VisitRepository(_context, _cache);
    public IUserLoyaltyRepository UserLoyalties => field ??= new UserLoyaltyRepository(_context, _cache);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        await _context.SaveChangesAsync(cancellationToken);

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default) =>
        await _context.Database.BeginTransactionAsync(cancellationToken);

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default) =>
        await _context.Database.CommitTransactionAsync(cancellationToken);

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default) =>
        await _context.Database.RollbackTransactionAsync(cancellationToken);
}
