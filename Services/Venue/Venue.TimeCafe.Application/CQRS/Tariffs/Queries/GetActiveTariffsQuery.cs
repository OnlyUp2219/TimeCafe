namespace Venue.TimeCafe.Application.CQRS.Tariffs.Queries;

public record GetActiveTariffsQuery() : IQuery<IEnumerable<TariffWithThemeDto>>;

public class GetActiveTariffsQueryHandler(IUnitOfWork uow) : IQueryHandler<GetActiveTariffsQuery, IEnumerable<TariffWithThemeDto>>
{
    private readonly IUnitOfWork _uow = uow;

    public async Task<Result<IEnumerable<TariffWithThemeDto>>> Handle(GetActiveTariffsQuery request, CancellationToken cancellationToken = default)
    {
        try
        {
            var tariffs = await _uow.Tariffs.GetActiveAsync(cancellationToken);
            return Result.Ok(tariffs);
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error("Внутренняя ошибка").CausedBy(ex));
        }
    }
}

