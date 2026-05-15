namespace Venue.TimeCafe.Application.CQRS.Themes.Queries;

public record GetThemesPageQuery(int Page, int PageSize) : IQuery<PagedResponse<ThemeDto>>;

public record ThemeDto(
    Guid ThemeId,
    string Name,
    string? Emoji,
    string? Colors);

public class GetThemesPageQueryHandler(IUnitOfWork uow) : IQueryHandler<GetThemesPageQuery, PagedResponse<ThemeDto>>
{
    private readonly IUnitOfWork _uow = uow;

    public async Task<Result<PagedResponse<ThemeDto>>> Handle(GetThemesPageQuery request, CancellationToken cancellationToken = default)
    {
        try
        {
            var themes = await _uow.Themes.GetPagedAsync(request.Page, request.PageSize, cancellationToken);
            var totalCount = await _uow.Themes.GetTotalCountAsync(cancellationToken);

            var dtos = themes.Select(t => new ThemeDto(
                t.ThemeId,
                t.Name,
                t.Emoji,
                t.Colors));

            var totalPages = (totalCount + request.PageSize - 1) / request.PageSize;

            return Result.Ok(new PagedResponse<ThemeDto>(
                dtos,
                new PageMetadata(request.Page, request.PageSize, totalCount, totalPages)));
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error("Внутренняя ошибка").CausedBy(ex));
        }
    }
}
