using TimeCafe.Core.Models;

namespace TimeCafe.Core.Contracts.Repositories;

public interface IBillingTypeRepository
{
    Task<IEnumerable<BillingType>> GetBillingTypesAsync();
    Task<BillingType> GetBillingTypeByIdAsync(int billingTypeId);
}