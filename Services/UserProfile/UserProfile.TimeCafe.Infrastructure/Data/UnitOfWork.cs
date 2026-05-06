namespace UserProfile.TimeCafe.Infrastructure.Data;

public class UnitOfWork(ApplicationDbContext context, HybridCache cache) : IUnitOfWork
{
    public IUserRepositories Profiles => field ??= new UserRepositories(context, cache);
    public IAdditionalInfoRepository AdditionalInfos => field ??= new AdditionalInfoRepository(context, cache);

    public Task<int> SaveChangesAsync(CancellationToken ct = default) => context.SaveChangesAsync(ct);
    public Task BeginTransactionAsync(CancellationToken ct = default) => context.Database.BeginTransactionAsync(ct);
    public Task CommitTransactionAsync(CancellationToken ct = default) => context.Database.CommitTransactionAsync(ct);
    public Task RollbackTransactionAsync(CancellationToken ct = default) => context.Database.RollbackTransactionAsync(ct);
}
