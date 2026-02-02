namespace Venue.TimeCafe.Application.CQRS.Themes.Queries;

public record GetThemeByIdQuery(string ThemeId) : IRequest<GetThemeByIdResult>;

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
            .NotEmpty().WithMessage("Тема не найдена")
           .NotNull().WithMessage("Тема не найдена")
            .Must(x => Guid.TryParse(x, out var guid) && guid != Guid.Empty).WithMessage("Тема не найдена");
    }
}

public class GetThemeByIdQueryHandler(IThemeRepository repository) : IRequestHandler<GetThemeByIdQuery, GetThemeByIdResult>
{
    private readonly IThemeRepository _repository = repository;

    public async Task<GetThemeByIdResult> Handle(GetThemeByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var themeId = Guid.Parse(request.ThemeId);

            var theme = await _repository.GetByIdAsync(themeId);

            if (theme == null)
                return GetThemeByIdResult.ThemeNotFound();

            return GetThemeByIdResult.GetSuccess(theme);
        }
        catch (Exception ex)
        {
            throw new CqrsResultException(GetThemeByIdResult.GetFailed(), ex);
        }
    }
}
