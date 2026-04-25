namespace Venue.TimeCafe.Application.CQRS.Themes.Queries;

public record GetThemeByIdQuery(Guid ThemeId) : IQuery<Theme>;

public class GetThemeByIdQueryValidator : AbstractValidator<GetThemeByIdQuery>
{
    public GetThemeByIdQueryValidator()
    {
        RuleFor(x => x.ThemeId).ValidGuidEntityId("Тема не найдена");
    }
}

public class GetThemeByIdQueryHandler(IThemeRepository repository) : IQueryHandler<GetThemeByIdQuery, Theme>
{
    private readonly IThemeRepository _repository = repository;

    public async Task<Result<Theme>> Handle(GetThemeByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var theme = await _repository.GetByIdAsync(request.ThemeId);

            if (theme == null)
                return Result.Fail(new ThemeNotFoundError());

            return Result.Ok(theme);
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error("Внутренняя ошибка").CausedBy(ex));
        }
    }
}

