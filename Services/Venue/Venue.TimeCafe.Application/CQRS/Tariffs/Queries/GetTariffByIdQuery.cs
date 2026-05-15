namespace Venue.TimeCafe.Application.CQRS.Tariffs.Queries;

public record GetTariffByIdQuery(Guid TariffId) : IQuery<TariffWithThemeDto>;

public class GetTariffByIdQueryHandler(IUnitOfWork uow) : IQueryHandler<GetTariffByIdQuery, TariffWithThemeDto>
{
    private readonly IUnitOfWork _uow = uow;

    public async Task<Result<TariffWithThemeDto>> Handle(GetTariffByIdQuery request, CancellationToken cancellationToken = default)
    {
        try
        {
            var tariff = await _uow.Tariffs.GetWithThemeByIdAsync(request.TariffId, cancellationToken);

            if (tariff == null)
                return Result.Fail(new TariffNotFoundError());

            tariff.CalculationExamples = GenerateCalculationExamples(tariff);

            return Result.Ok(tariff);
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error("Внутренняя ошибка").CausedBy(ex));
        }
    }

    private static List<CostBreakdownDto> GenerateCalculationExamples(TariffWithThemeDto tariff)
    {
        var examples = new List<CostBreakdownDto>();
        var durations = new[] { 30, 60, 120 };
        var now = DateTimeOffset.UtcNow;

        foreach (var duration in durations)
        {
            var breakdown = Visit.CalculateCost(
                tariffBillingType: tariff.BillingType,
                tariffPricePerMinute: tariff.PricePerMinute,
                exitTime: now.AddMinutes(duration),
                entryTime: now,
                minSessionMinutes: tariff.MinSessionMinutes,
                roundingRule: tariff.RoundingRule
            );

            examples.Add(new CostBreakdownDto
            {
                ActualMinutes = breakdown.ActualMinutes,
                BillableMinutes = breakdown.BillableMinutes,
                BaseCost = breakdown.BaseCost,
                FinalCost = breakdown.FinalCost,
                OptimizationGain = breakdown.OptimizationGain
            });
        }

        return examples;
    }
}

