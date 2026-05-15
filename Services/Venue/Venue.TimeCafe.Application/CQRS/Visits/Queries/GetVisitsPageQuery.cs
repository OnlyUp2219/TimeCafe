using BuildingBlocks.Contracts.CQRS;

namespace Venue.TimeCafe.Application.CQRS.Visits.Queries;

public record GetVisitsPageQuery(int Page, int PageSize) : IQuery<PagedResponse<VisitWithTariffDto>>;

public class GetVisitsPageQueryHandler(IUnitOfWork uow) : IQueryHandler<GetVisitsPageQuery, PagedResponse<VisitWithTariffDto>>
{
    private readonly IUnitOfWork _uow = uow;

    public async Task<Result<PagedResponse<VisitWithTariffDto>>> Handle(GetVisitsPageQuery request, CancellationToken cancellationToken = default)
    {
        try
        {
            var visits = await _uow.Visits.GetPagedAsync(request.Page, request.PageSize, cancellationToken);
            var totalCount = await _uow.Visits.GetTotalCountAsync(cancellationToken);
            var totalPages = (totalCount + request.PageSize - 1) / request.PageSize;

            return Result.Ok(new PagedResponse<VisitWithTariffDto>(
                visits, 
                new PageMetadata(request.Page, request.PageSize, totalCount, totalPages)));
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error("Внутренняя ошибка").CausedBy(ex));
        }
    }
}
