namespace Venue.TimeCafe.Test.Integration.Mocks;

public class MockVisitBalancePolicyService : IVisitBalancePolicyService
{
    public Task<VisitBalanceCheckResult> CheckBeforeStartAsync(
        Guid userId,
        TariffWithThemeDto tariff,
        int? plannedMinutes,
        bool requirePositiveBalance,
        bool requireEnoughForPlanned,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(new VisitBalanceCheckResult(true));
    }
}
