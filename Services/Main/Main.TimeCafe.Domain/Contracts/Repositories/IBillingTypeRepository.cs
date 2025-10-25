namespace Main.TimeCafe.Domain.Contracts.Repositories;

public interface IBillingTypeRepository
{
    Task<IEnumerable<BillingType>> GetBillingTypesAsync();
    Task<BillingType> GetBillingTypeByIdAsync(int billingTypeId);
}