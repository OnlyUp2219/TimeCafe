using System.Net.Http.Json;

namespace Venue.TimeCafe.Infrastructure.Services;

public class VisitBalancePolicyService(IHttpClientFactory httpClientFactory, ILogger<VisitBalancePolicyService> logger)
    : IVisitBalancePolicyService
{
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly ILogger<VisitBalancePolicyService> _logger = logger;

    public async Task<VisitBalanceCheckResult> CheckBeforeStartAsync(
        Guid userId,
        TariffWithThemeDto tariff,
        int? plannedMinutes,
        bool requirePositiveBalance,
        bool requireEnoughForPlanned,
        CancellationToken cancellationToken)
    {
        var client = _httpClientFactory.CreateClient("BillingApi");
        var response = await client.GetFromJsonAsync<GetBalanceResponse>($"/billing/balance/{userId}", cancellationToken);

        if (response?.Balance == null)
        {
            throw new HttpRequestException("Не удалось получить баланс пользователя");
        }

        var available = response.Balance.CurrentBalance - response.Balance.Debt;

        if (requirePositiveBalance && available <= 0)
        {
            return new VisitBalanceCheckResult(false, "Недостаточно средств для старта визита");
        }

        if (!requireEnoughForPlanned || !plannedMinutes.HasValue)
        {
            return new VisitBalanceCheckResult(true);
        }

        var estimated = EstimateVisitCost(tariff, plannedMinutes.Value);
        if (available < estimated)
        {
            _logger.LogInformation(
                "Недостаточно средств для старта визита. UserId={UserId}, Available={Available}, Estimated={Estimated}",
                userId,
                available,
                estimated);

            return new VisitBalanceCheckResult(
                false,
                $"Недостаточно средств на выбранное время. Нужно: {estimated:F2}, доступно: {available:F2}");
        }

        return new VisitBalanceCheckResult(true);
    }

    private static decimal EstimateVisitCost(TariffWithThemeDto tariff, int plannedMinutes)
    {
        var minutes = Math.Max(1, plannedMinutes);
        var pricePerMinute = Math.Max(0, tariff.PricePerMinute);

        if (tariff.BillingType == BillingType.PerMinute)
        {
            return minutes * pricePerMinute;
        }

        var billedHours = Math.Max(1, (int)Math.Ceiling(minutes / 60m));
        return billedHours * pricePerMinute * 60m;
    }

    private sealed class GetBalanceResponse
    {
        public BalancePayload? Balance { get; set; }
    }

    private sealed class BalancePayload
    {
        public decimal CurrentBalance { get; set; }
        public decimal Debt { get; set; }
    }
}
