namespace Venue.TimeCafe.Application.CQRS.Visits.Queries;

public record GetPendingVisitsQuery(int PageNumber = 1, int PageSize = 20) : IQuery<PagedResponse<VisitWithTariffDto>>;

public class GetPendingVisitsQueryHandler(IUnitOfWork uow) : IQueryHandler<GetPendingVisitsQuery, PagedResponse<VisitWithTariffDto>>
{
    private readonly IUnitOfWork _uow = uow;

    public async Task<Result<PagedResponse<VisitWithTariffDto>>> Handle(GetPendingVisitsQuery request, CancellationToken cancellationToken = default)
    {
        try
        {
            var items = await _uow.Visits.GetPendingVisitsAsync(request.PageNumber, request.PageSize, cancellationToken);
            var totalCount = await _uow.Visits.GetPendingCountAsync(cancellationToken);

            var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);
            return Result.Ok(new PagedResponse<VisitWithTariffDto>(
                items,
                new PageMetadata(request.PageNumber, request.PageSize, totalCount, totalPages)
            ));
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error("Внутренняя ошибка").CausedBy(ex));
        }
    }
}
