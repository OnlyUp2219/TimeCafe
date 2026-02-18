namespace Venue.TimeCafe.Application.Contracts.Services;

public record VisitBalanceCheckResult(bool IsAllowed, string? Message = null);

public interface IVisitBalancePolicyService
{
    Task<VisitBalanceCheckResult> CheckBeforeStartAsync(
        Guid userId,
        TariffWithThemeDto tariff,
        int? plannedMinutes,
        bool requirePositiveBalance,
        bool requireEnoughForPlanned,
        CancellationToken cancellationToken);
}
