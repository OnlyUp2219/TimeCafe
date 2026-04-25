namespace Venue.TimeCafe.Application.CQRS.Themes.Queries;

public record GetAllThemesQuery() : IQuery<IEnumerable<Theme>>;

public class GetAllThemesQueryValidator : AbstractValidator<GetAllThemesQuery>
{
    public GetAllThemesQueryValidator()
    {
    }
}

public class GetAllThemesQueryHandler(IThemeRepository repository) : IQueryHandler<GetAllThemesQuery, IEnumerable<Theme>>
{
    private readonly IThemeRepository _repository = repository;

    public async Task<Result<IEnumerable<Theme>>> Handle(GetAllThemesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var themes = await _repository.GetAllAsync();
            return Result.Ok(themes);
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error("Внутренняя ошибка").CausedBy(ex));
        }
    }
}

