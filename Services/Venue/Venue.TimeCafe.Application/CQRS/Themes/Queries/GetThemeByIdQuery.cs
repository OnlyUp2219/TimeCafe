namespace Venue.TimeCafe.Application.CQRS.Themes.Queries;

public record GetThemeByIdQuery(int ThemeId) : IRequest<GetThemeByIdResult>;

public record GetThemeByIdResult(
    bool Success,
    string? Code = null,
    string? Message = null,
    int? StatusCode = null,
    List<ErrorItem>? Errors = null,
    Theme? Theme = null) : ICqrsResultV2
{
    public static GetThemeByIdResult ThemeNotFound() =>
        new(false, Code: "ThemeNotFound", Message: "Тема не найдена", StatusCode: 404);

    public static GetThemeByIdResult GetFailed() =>
        new(false, Code: "GetThemeFailed", Message: "Не удалось получить тему", StatusCode: 500);

    public static GetThemeByIdResult GetSuccess(Theme theme) =>
        new(true, Theme: theme);
}

public class GetThemeByIdQueryValidator : AbstractValidator<GetThemeByIdQuery>
{
    public GetThemeByIdQueryValidator()
    {
        RuleFor(x => x.ThemeId)
            .GreaterThan(0).WithMessage("ID темы обязателен");
    }
}

public class GetThemeByIdQueryHandler(IThemeRepository repository) : IRequestHandler<GetThemeByIdQuery, GetThemeByIdResult>
{
    private readonly IThemeRepository _repository = repository;

    public async Task<GetThemeByIdResult> Handle(GetThemeByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var theme = await _repository.GetByIdAsync(request.ThemeId);

            if (theme == null)
                return GetThemeByIdResult.ThemeNotFound();

            return GetThemeByIdResult.GetSuccess(theme);
        }
        catch (Exception)
        {
            return GetThemeByIdResult.GetFailed();
        }
    }
}
