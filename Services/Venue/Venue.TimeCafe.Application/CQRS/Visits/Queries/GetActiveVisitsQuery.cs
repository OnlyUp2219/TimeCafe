namespace Venue.TimeCafe.Application.CQRS.Visits.Queries;

public record GetActiveVisitsQuery() : IQuery<IEnumerable<VisitWithTariffDto>>;

public class GetActiveVisitsQueryHandler(IUnitOfWork uow) : IQueryHandler<GetActiveVisitsQuery, IEnumerable<VisitWithTariffDto>>
{
    private readonly IUnitOfWork _uow = uow;

    public async Task<Result<IEnumerable<VisitWithTariffDto>>> Handle(GetActiveVisitsQuery request, CancellationToken cancellationToken = default)
    {
        try
        {
            var visits = await _uow.Visits.GetActiveVisitsAsync(cancellationToken);
            return Result.Ok(visits);
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error("Внутренняя ошибка").CausedBy(ex));
        }
    }
}

