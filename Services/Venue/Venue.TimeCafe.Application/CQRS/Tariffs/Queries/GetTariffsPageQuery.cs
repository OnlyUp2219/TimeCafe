namespace Venue.TimeCafe.Application.CQRS.Tariffs.Queries;

public record GetTariffsPageQuery(int Page, int PageSize) : IQuery<GetTariffsPageResponse>;

public record GetTariffsPageResponse(
    IEnumerable<TariffWithThemeDto> Items, 
    PageMetadata Metadata,
    decimal MaxTotalDiscountPercent);

public class GetTariffsPageQueryHandler(IUnitOfWork uow, IOptionsSnapshot<VenuePricingOptions> options) : IQueryHandler<GetTariffsPageQuery, GetTariffsPageResponse>
{
    private readonly IUnitOfWork _uow = uow;
    private readonly VenuePricingOptions _options = options.Value;

    public async Task<Result<GetTariffsPageResponse>> Handle(GetTariffsPageQuery request, CancellationToken cancellationToken = default)
    {
        try
        {
            var tariffs = await _uow.Tariffs.GetPagedAsync(request.Page, request.PageSize, cancellationToken);
            var totalCount = await _uow.Tariffs.GetTotalCountAsync(cancellationToken);
            var totalPages = (totalCount + request.PageSize - 1) / request.PageSize;

            return Result.Ok(new GetTariffsPageResponse(
                tariffs, 
                new PageMetadata(request.Page, request.PageSize, totalCount, totalPages), 
                _options.MaxTotalDiscountPercent));
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error("Внутренняя ошибка").CausedBy(ex));
        }
    }
}
