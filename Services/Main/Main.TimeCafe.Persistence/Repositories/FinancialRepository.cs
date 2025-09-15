namespace TimeCafe.Persistence.Repositories;

public class FinancialRepository : IFinancialRepository
{
    private readonly TimeCafeContext _context;
    private readonly IDistributedCache _cache;
    private readonly ILogger<FinancialRepository> _logger;

    public FinancialRepository(TimeCafeContext context, IDistributedCache cache, ILogger<FinancialRepository> logger)
    {
        _context = context;
        _cache = cache;
        _logger = logger;
    }

    public async Task<decimal> GetClientBalanceAsync(int clientId)
    {
        var cached = await CacheHelper.GetAsync<decimal?>(
            _cache,
            _logger,
            CacheKeys.FinancialTransaction_ByClientId(clientId));
        if (cached.HasValue)
            return cached.Value;

        var transactions = await _context.FinancialTransactions
            .Where(t => t.ClientId == clientId)
            .ToListAsync();

        var balance = transactions.Sum(t => t.TransactionTypeId == 1 ? t.Amount : -t.Amount);

        await CacheHelper.SetAsync(
            _cache,
            _logger,
            CacheKeys.FinancialTransaction_ByClientId(clientId),
            balance);

        return balance;
    }

    public async Task<IEnumerable<FinancialTransaction>> GetClientTransactionsAsync(int clientId, int? limit = null)
    {
        var cached = await CacheHelper.GetAsync<IEnumerable<FinancialTransaction>>(
            _cache,
            _logger,
            CacheKeys.FinancialTransaction_ByClientId(clientId));
        if (cached != null)
            return cached;

        var query = _context.FinancialTransactions
            .Include(t => t.TransactionType)
            .Include(t => t.Visit)
            .Where(t => t.ClientId == clientId)
            .OrderByDescending(t => t.TransactionDate);

        if (limit.HasValue)
        {
            var clientTransLimit = await query.Take(limit.Value).ToListAsync();
            //TODO : Решить, что делать с кэшированием 
            return clientTransLimit;
        }

        var clientTrans = await query.ToListAsync();

        await CacheHelper.SetAsync(
            _cache,
            _logger,
            CacheKeys.FinancialTransaction_ByClientId(clientId),
            clientTrans);

        return clientTrans;
    }

    public async Task<decimal> GetClientDebtAsync(int clientId)
    {
        var balance = await GetClientBalanceAsync(clientId);
        return balance < 0 ? Math.Abs(balance) : 0;
    }

    public async Task<IEnumerable<ClientBalanceDto>> GetAllClientsBalancesAsync()
    {
        var clients = await _context.Clients
            .Include(c => c.Status)
            .Include(c => c.FinancialTransactions)
            .ToListAsync();

        var result = clients.Select(client =>
        {
            var balance = client.FinancialTransactions.Sum(t =>
                t.TransactionTypeId == 1 ? t.Amount : -t.Amount);

            var lastTransactionDate = client.FinancialTransactions
                .OrderByDescending(t => t.TransactionDate)
                .Select(t => t.TransactionDate)
                .FirstOrDefault();

            return new ClientBalanceDto(
                client.ClientId,
                $"{client.LastName} {client.FirstName} {client.MiddleName}".Trim(),
                client.PhoneNumber,
                balance,
                balance < 0 ? Math.Abs(balance) : 0,
                lastTransactionDate == default ? client.CreatedAt : lastTransactionDate,
                client.Status?.StatusName == "Активный"
            );
        });

        return result
            .OrderByDescending(c => c.Debt)
            .ThenBy(c => c.FullName)
            .ToList();
    }

    public async Task<IEnumerable<ClientBalanceDto>> GetClientsWithDebtAsync()
    {
        var allClients = await GetAllClientsBalancesAsync();
        return allClients.Where(c => c.Debt > 0);
    }

    public async Task<bool> HasSufficientBalanceAsync(int clientId, decimal requiredAmount)
    {
        var balance = await GetClientBalanceAsync(clientId);
        return balance >= requiredAmount;
    }


    public async Task<FinancialTransaction> DepositAsync(int clientId, decimal amount, string comment = null)
    {
        if (amount <= 0)
            throw new ArgumentException("Сумма пополнения должна быть больше 0");

        var transaction = new FinancialTransaction
        {
            ClientId = clientId,
            Amount = amount,
            TransactionTypeId = 1,
            TransactionDate = DateTime.Now,
            Comment = comment
        };

        _context.FinancialTransactions.Add(transaction);
        await _context.SaveChangesAsync();

        await CacheHelper.RemoveKeysAsync(
            _cache,
            _logger,
            CacheKeys.FinancialTransaction_All,
            CacheKeys.FinancialTransaction_ById(transaction.TransactionId),
            CacheKeys.FinancialTransaction_ByClientId(clientId));

        return transaction;
    }

    public async Task<FinancialTransaction> DeductAsync(int clientId, decimal amount, int? visitId = null, string comment = null)
    {
        if (amount <= 0)
            throw new ArgumentException("Сумма списания должна быть больше 0");

        var transaction = new FinancialTransaction
        {
            ClientId = clientId,
            Amount = amount,
            TransactionTypeId = 2,
            TransactionDate = DateTime.Now,
            VisitId = visitId,
            Comment = comment
        };

        _context.FinancialTransactions.Add(transaction);
        await _context.SaveChangesAsync();

        await CacheHelper.RemoveKeysAsync(
          _cache,
          _logger,
          CacheKeys.FinancialTransaction_All,
          CacheKeys.FinancialTransaction_ById(transaction.TransactionId),
          CacheKeys.FinancialTransaction_ByClientId(clientId));

        return transaction;
    }
}