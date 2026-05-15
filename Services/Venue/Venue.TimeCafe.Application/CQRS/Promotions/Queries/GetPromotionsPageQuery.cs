namespace Venue.TimeCafe.Application.CQRS.Promotions.Queries;

public record GetPromotionsPageQuery(int Page, int PageSize) : IQuery<PagedResponse<PromotionDto>>;

public record PromotionDto(
    Guid PromotionId,
    string Name,
    string Description,
    decimal? DiscountPercent,
    DateTimeOffset ValidFrom,
    DateTimeOffset ValidTo,
    bool IsActive,
    int Type,
    Guid? TariffId,
    DateTimeOffset CreatedAt);

public class GetPromotionsPageQueryHandler(IUnitOfWork uow) : IQueryHandler<GetPromotionsPageQuery, PagedResponse<PromotionDto>>
{
    private readonly IUnitOfWork _uow = uow;

    public async Task<Result<PagedResponse<PromotionDto>>> Handle(GetPromotionsPageQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var promotions = await _uow.Promotions.GetPagedAsync(request.Page, request.PageSize, cancellationToken);
            var totalCount = await _uow.Promotions.GetTotalCountAsync(cancellationToken);

            var dtos = promotions.Select(p => new PromotionDto(
                p.PromotionId,
                p.Name,
                p.Description,
                p.DiscountPercent,
                p.ValidFrom,
                p.ValidTo,
                p.IsActive,
                (int)p.Type,
                p.TariffId,
                p.CreatedAt));

            var totalPages = (totalCount + request.PageSize - 1) / request.PageSize;

            return Result.Ok(new PagedResponse<PromotionDto>(
                dtos,
                new PageMetadata(request.Page, request.PageSize, totalCount, totalPages)));
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error("Внутренняя ошибка").CausedBy(ex));
        }
    }
}
