namespace Venue.TimeCafe.Application.CQRS.Tariffs.Queries;

public record GetTariffsByBillingTypeQuery(BillingType BillingType) : IQuery<IEnumerable<TariffWithThemeDto>>;

public class GetTariffsByBillingTypeQueryHandler(ITariffRepository repository) : IQueryHandler<GetTariffsByBillingTypeQuery, IEnumerable<TariffWithThemeDto>>
{
    private readonly ITariffRepository _repository = repository;

    public async Task<Result<IEnumerable<TariffWithThemeDto>>> Handle(GetTariffsByBillingTypeQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var tariffs = await _repository.GetByBillingTypeAsync(request.BillingType);
            return Result.Ok(tariffs);
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error("Внутренняя ошибка").CausedBy(ex));
        }
    }
}

public class GetTariffsByBillingTypeQueryValidator : AbstractValidator<GetTariffsByBillingTypeQuery>
{
    public GetTariffsByBillingTypeQueryValidator()
    {
    }
}

