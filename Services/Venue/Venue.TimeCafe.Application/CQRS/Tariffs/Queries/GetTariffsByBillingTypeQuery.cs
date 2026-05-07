namespace Venue.TimeCafe.Application.CQRS.Tariffs.Queries;

public record GetTariffsByBillingTypeQuery(BillingType BillingType) : IQuery<IEnumerable<TariffWithThemeDto>>;

public class GetTariffsByBillingTypeQueryHandler(IUnitOfWork uow) : IQueryHandler<GetTariffsByBillingTypeQuery, IEnumerable<TariffWithThemeDto>>
{
    private readonly IUnitOfWork _uow = uow;

    public async Task<Result<IEnumerable<TariffWithThemeDto>>> Handle(GetTariffsByBillingTypeQuery request, CancellationToken cancellationToken = default)
    {
        try
        {
            var tariffs = await _uow.Tariffs.GetByBillingTypeAsync(request.BillingType, cancellationToken);
            return Result.Ok(tariffs);
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error("Внутренняя ошибка").CausedBy(ex));
        }
    }
}
