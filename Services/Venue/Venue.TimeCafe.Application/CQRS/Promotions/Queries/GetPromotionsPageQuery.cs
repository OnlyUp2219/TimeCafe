namespace Venue.TimeCafe.Application.CQRS.Promotions.Queries;

public record GetPromotionsPageQuery(int PageNumber, int PageSize) : IQuery<GetPromotionsPageResponse>;

public record GetPromotionsPageResponse(IEnumerable<Promotion> Promotions, int TotalCount);

public class GetPromotionsPageQueryValidator : AbstractValidator<GetPromotionsPageQuery>
{
    public GetPromotionsPageQueryValidator()
    {
        RuleFor(x => x.PageNumber).ValidPageNumber();
        RuleFor(x => x.PageSize).ValidPageSize();
    }
}

public class GetPromotionsPageQueryHandler(IPromotionRepository repository) : IQueryHandler<GetPromotionsPageQuery, GetPromotionsPageResponse>
{
    private readonly IPromotionRepository _repository = repository;

    public async Task<Result<GetPromotionsPageResponse>> Handle(GetPromotionsPageQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var promotions = await _repository.GetPagedAsync(request.PageNumber, request.PageSize, cancellationToken);
            var totalCount = await _repository.GetTotalCountAsync(cancellationToken);

            return Result.Ok(new GetPromotionsPageResponse(promotions, totalCount));
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error("Внутренняя ошибка").CausedBy(ex));
        }
    }
}
