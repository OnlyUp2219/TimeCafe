using TimeCafe.Core.Models;

namespace TimeCafe.Core.Contracts.Repositories;

public interface IFinancialRepository
{
    // Queries
    Task<decimal> GetClientBalanceAsync(int clientId);
    Task<IEnumerable<FinancialTransaction>> GetClientTransactionsAsync(int clientId, int? limit = null);
    Task<decimal> GetClientDebtAsync(int clientId);
    Task<bool> HasSufficientBalanceAsync(int clientId, decimal requiredAmount);
    Task<IEnumerable<ClientBalanceDto>> GetAllClientsBalancesAsync();
    Task<IEnumerable<ClientBalanceDto>> GetClientsWithDebtAsync();

    // Commands
    Task<FinancialTransaction> DepositAsync(int clientId, decimal amount, string comment = null);
    Task<FinancialTransaction> DeductAsync(int clientId, decimal amount, int? visitId = null, string comment = null);
}