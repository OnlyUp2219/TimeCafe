namespace Venue.TimeCafe.Application.CQRS.Promotions.Queries;

public record GetPromotionsPageQuery(int PageNumber, int PageSize) : IQuery<GetPromotionsPageResponse>;

public record GetPromotionsPageResponse(IEnumerable<Promotion> Promotions, int TotalCount);

public class GetPromotionsPageQueryHandler(IUnitOfWork uow) : IQueryHandler<GetPromotionsPageQuery, GetPromotionsPageResponse>
{
    private readonly IUnitOfWork _uow = uow;

    public async Task<Result<GetPromotionsPageResponse>> Handle(GetPromotionsPageQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var promotions = await _uow.Promotions.GetPagedAsync(request.PageNumber, request.PageSize, cancellationToken);
            var totalCount = await _uow.Promotions.GetTotalCountAsync(cancellationToken);

            return Result.Ok(new GetPromotionsPageResponse(promotions, totalCount));
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error("Внутренняя ошибка").CausedBy(ex));
        }
    }
}
