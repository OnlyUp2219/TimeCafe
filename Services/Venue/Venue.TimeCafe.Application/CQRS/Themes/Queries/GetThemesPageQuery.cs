namespace Venue.TimeCafe.Application.CQRS.Themes.Queries;

public record GetThemesPageQuery(int PageNumber, int PageSize) : IQuery<GetThemesPageResponse>;

public record GetThemesPageResponse(IEnumerable<Theme> Themes, int TotalCount);

public class GetThemesPageQueryHandler(IUnitOfWork uow) : IQueryHandler<GetThemesPageQuery, GetThemesPageResponse>
{
    private readonly IUnitOfWork _uow = uow;

    public async Task<Result<GetThemesPageResponse>> Handle(GetThemesPageQuery request, CancellationToken cancellationToken = default)
    {
        try
        {
            var themes = await _uow.Themes.GetPagedAsync(request.PageNumber, request.PageSize, cancellationToken);
            var totalCount = await _uow.Themes.GetTotalCountAsync(cancellationToken);

            return Result.Ok(new GetThemesPageResponse(themes, totalCount));
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error("Внутренняя ошибка").CausedBy(ex));
        }
    }
}
