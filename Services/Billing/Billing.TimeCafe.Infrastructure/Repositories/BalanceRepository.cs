namespace Billing.TimeCafe.Infrastructure.Repositories;

public class BalanceRepository(
    ApplicationDbContext context,
    HybridCache cache) : IBalanceRepository
{
    private readonly ApplicationDbContext _context = context;
    private readonly HybridCache _cache = cache;

    public async Task<Balance?> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        return await _cache.GetOrCreateAsync(
            CacheKeys.Balance_ByUserId(userId),
            async token => await _context.Balances
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.UserId == userId, token),
            tags: [CacheTags.Balances, CacheTags.Balance(userId)],
            cancellationToken: ct);
    }

    //   Microsoft.EntityFrameworkCore.DbUpdateException
    //    HResult=0x80131500
    //  Сообщение = An error occurred while saving the entity changes.See the inner exception for details.
    //  Источник = Microsoft.EntityFrameworkCore.Relational
    //  Трассировка стека:
    //   в Microsoft.EntityFrameworkCore.Update.ReaderModificationCommandBatch.<ExecuteAsync>d__50.MoveNext()
    //   в Microsoft.EntityFrameworkCore.Update.Internal.BatchExecutor.<ExecuteAsync>d__9.MoveNext()
    //   в Microsoft.EntityFrameworkCore.Update.Internal.BatchExecutor.<ExecuteAsync>d__9.MoveNext()
    //   в Microsoft.EntityFrameworkCore.Update.Internal.BatchExecutor.<ExecuteAsync>d__9.MoveNext()
    //   в Microsoft.EntityFrameworkCore.Storage.RelationalDatabase.<SaveChangesAsync>d__8.MoveNext()
    //   в Microsoft.EntityFrameworkCore.ChangeTracking.Internal.StateManager.<SaveChangesAsync>d__113.MoveNext()
    //   в Microsoft.EntityFrameworkCore.ChangeTracking.Internal.StateManager.<SaveChangesAsync>d__117.MoveNext()
    //   в Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.NpgsqlExecutionStrategy.<ExecuteAsync>d__7`2.MoveNext()
    //   в Microsoft.EntityFrameworkCore.DbContext.<SaveChangesAsync>d__63.MoveNext()
    //   в Microsoft.EntityFrameworkCore.DbContext.<SaveChangesAsync>d__63.MoveNext()
    //   в Billing.TimeCafe.Infrastructure.Repositories.BalanceRepository.<CreateAsync>d__4.MoveNext() в D:\IT\TimeCafe\Services\Billing\Billing.TimeCafe.Infrastructure\Repositories\BalanceRepository.cs:строка 28
    //   в Billing.TimeCafe.Application.CQRS.Balances.Queries.GetUserDebtQueryHandler.<Handle>d__2.MoveNext() в D:\IT\TimeCafe\Services\Billing\Billing.TimeCafe.Application\CQRS\Balances\Queries\GetUserDebtQuery.cs:строка 39
    //   в BuildingBlocks.Behaviors.ErrorHandlingBehavior`2.<Handle>d__2.MoveNext() в D:\IT\TimeCafe\Services\BuildingBlocks\Behaviors\ErrorHandlingBehavior.cs:строка 12
    //   в BuildingBlocks.Behaviors.PerformanceBehavior`2.<Handle>d__3.MoveNext() в D:\IT\TimeCafe\Services\BuildingBlocks\Behaviors\PerformanceBehavior.cs:строка 12
    //   в BuildingBlocks.Behaviors.LoggingBehavior`2.<Handle>d__3.MoveNext() в D:\IT\TimeCafe\Services\BuildingBlocks\Behaviors\LoggingBehavior.cs:строка 29
    //   в BuildingBlocks.Behaviors.ValidationBehavior`2.<Handle>d__3.MoveNext() в D:\IT\TimeCafe\Services\BuildingBlocks\Behaviors\ValidationBehavior.cs:строка 30
    //   в MediatR.Pipeline.RequestExceptionProcessorBehavior`2.<Handle>d__2.MoveNext()

    //  Изначально это исключение было создано в этом стеке вызовов: 
    //    [Внешний код]

    //Внутреннее исключение 1:
    //PostgresException: 23505: duplicate key value violates unique constraint "PK_Balances"

    //DETAIL: Detail redacted as it may contain sensitive data.Specify 'Include Error Detail' in the connection string to include this information.
    public async Task<Balance> CreateAsync(Balance balance, CancellationToken ct = default)
    {
        var existingBalance = await GetByUserIdAsync(balance.UserId, ct);
        if (existingBalance != null)
            return existingBalance;

        _context.Balances.Add(balance);
        await _context.SaveChangesAsync(ct);

        await _cache.RemoveByTagAsync(CacheTags.Balances, ct);

        return balance;
    }

    public async Task<Balance> UpdateAsync(Balance balance, CancellationToken ct = default)
    {
        _context.Balances.Update(balance);
        await _context.SaveChangesAsync(ct);

        await _cache.RemoveByTagAsync(CacheTags.Balances, ct);
        await _cache.RemoveByTagAsync(CacheTags.Transactions, ct);
        await _cache.RemoveByTagAsync(CacheTags.TransactionByUser(balance.UserId), ct);

        return balance;
    }

    public async Task<bool> ExistsAsync(Guid userId, CancellationToken ct = default)
    {
        return await _context.Balances
            .AnyAsync(b => b.UserId == userId, ct);
    }

    public async Task<List<Balance>> GetUsersWithDebtAsync(CancellationToken ct = default)
    {
        return await _cache.GetOrCreateAsync(
            CacheKeys.Debtors_All,
            async token => await _context.Balances
                .AsNoTracking()
                .Where(b => b.Debt > 0)
                .OrderByDescending(b => b.Debt)
                .ToListAsync(token),
            new HybridCacheEntryOptions { Expiration = TimeSpan.FromMinutes(10) },
            tags: [CacheTags.Balances],
            cancellationToken: ct);
    }
}
