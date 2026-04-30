namespace Venue.TimeCafe.Application.CQRS.Themes.Queries;

public record GetThemesPageQuery(int PageNumber, int PageSize) : IQuery<GetThemesPageResponse>;

public record GetThemesPageResponse(IEnumerable<Theme> Themes, int TotalCount);

public class GetThemesPageQueryValidator : AbstractValidator<GetThemesPageQuery>
{
    public GetThemesPageQueryValidator()
    {
        RuleFor(x => x.PageNumber).ValidPageNumber();
        RuleFor(x => x.PageSize).ValidPageSize();
    }
}

public class GetThemesPageQueryHandler(IThemeRepository repository) : IQueryHandler<GetThemesPageQuery, GetThemesPageResponse>
{
    private readonly IThemeRepository _repository = repository;

    public async Task<Result<GetThemesPageResponse>> Handle(GetThemesPageQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var themes = await _repository.GetPagedAsync(request.PageNumber, request.PageSize, cancellationToken);
            var totalCount = await _repository.GetTotalCountAsync(cancellationToken);

            return Result.Ok(new GetThemesPageResponse(themes, totalCount));
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error("Внутренняя ошибка").CausedBy(ex));
        }
    }
}
