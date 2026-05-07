namespace Venue.TimeCafe.Application.CQRS.Tariffs.Queries;

public record GetAllTariffsQuery() : IQuery<IEnumerable<TariffWithThemeDto>>;

public class GetAllTariffsQueryHandler(IUnitOfWork uow) : IQueryHandler<GetAllTariffsQuery, IEnumerable<TariffWithThemeDto>>
{
    private readonly IUnitOfWork _uow = uow;

    public async Task<Result<IEnumerable<TariffWithThemeDto>>> Handle(GetAllTariffsQuery request, CancellationToken cancellationToken = default)
    {
        try
        {
            var tariffs = await _uow.Tariffs.GetAllAsync(cancellationToken);
            return Result.Ok(tariffs);
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error("Внутренняя ошибка").CausedBy(ex));
        }
    }
}

