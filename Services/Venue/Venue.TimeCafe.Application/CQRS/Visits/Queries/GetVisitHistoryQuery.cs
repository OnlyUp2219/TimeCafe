namespace Venue.TimeCafe.Application.CQRS.Visits.Queries;

public record GetVisitHistoryQuery(Guid UserId, int PageNumber, int PageSize) : IQuery<IEnumerable<VisitWithTariffDto>>;

public class GetVisitHistoryQueryHandler(IUnitOfWork uow) : IQueryHandler<GetVisitHistoryQuery, IEnumerable<VisitWithTariffDto>>
{
    private readonly IUnitOfWork _uow = uow;

    public async Task<Result<IEnumerable<VisitWithTariffDto>>> Handle(GetVisitHistoryQuery request, CancellationToken cancellationToken = default)
    {
        try
        {
            var visits = await _uow.Visits.GetVisitHistoryByUserAsync(request.UserId, request.PageNumber, request.PageSize, cancellationToken);
            return Result.Ok(visits);
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error("Внутренняя ошибка").CausedBy(ex));
        }
    }
}

