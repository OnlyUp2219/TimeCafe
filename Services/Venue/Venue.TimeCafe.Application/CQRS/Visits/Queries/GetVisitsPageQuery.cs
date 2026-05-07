namespace Venue.TimeCafe.Application.CQRS.Visits.Queries;

public record GetVisitsPageQuery(int PageNumber, int PageSize) : IQuery<GetVisitsPageResponse>;

public record GetVisitsPageResponse(IEnumerable<VisitWithTariffDto> Visits, int TotalCount);




public class GetVisitsPageQueryHandler(IUnitOfWork uow) : IQueryHandler<GetVisitsPageQuery, GetVisitsPageResponse>
{
    private readonly IUnitOfWork _uow = uow;

    public async Task<Result<GetVisitsPageResponse>> Handle(GetVisitsPageQuery request, CancellationToken cancellationToken = default)
    {
        try
        {
            var visits = await _uow.Visits.GetPagedAsync(request.PageNumber, request.PageSize, cancellationToken);
            var totalCount = await _uow.Visits.GetTotalCountAsync(cancellationToken);

            return Result.Ok(new GetVisitsPageResponse(visits, totalCount));
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error("Внутренняя ошибка").CausedBy(ex));
        }
    }
}

