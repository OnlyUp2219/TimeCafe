namespace Venue.TimeCafe.Application.CQRS.Themes.Commands;

public record UpdateThemeCommand(string ThemeId, string Name, string? Emoji, string? Colors) : IRequest<UpdateThemeResult>;

public record UpdateThemeResult(
    bool Success,
    string? Code = null,
    string? Message = null,
    int? StatusCode = null,
    List<ErrorItem>? Errors = null,
    Theme? Theme = null) : ICqrsResultV2
{
    public static UpdateThemeResult ThemeNotFound() =>
        new(false, Code: "ThemeNotFound", Message: "Тема не найдена", StatusCode: 404);

    public static UpdateThemeResult UpdateFailed() =>
        new(false, Code: "UpdateThemeFailed", Message: "Не удалось обновить тему", StatusCode: 500);

    public static UpdateThemeResult UpdateSuccess(Theme theme) =>
        new(true, Message: "Тема успешно обновлена", Theme: theme);
}

public class UpdateThemeCommandValidator : AbstractValidator<UpdateThemeCommand>
{
    public UpdateThemeCommandValidator()
    {
        // TODO : finish up validators
        RuleFor(x => x.ThemeId)
            .NotEmpty().WithMessage("Тема не найдена")
           .NotNull().WithMessage("Тема не найдена")
            .Must(x => Guid.TryParse(x, out var guid) && guid != Guid.Empty).WithMessage("Тема не найдена");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Название темы обязательно")
            .MaximumLength(100).WithMessage("Название не может превышать 100 символов");

        RuleFor(x => x.Emoji)
            .MaximumLength(10).WithMessage("Эмодзи не может превышать 10 символов")
            .When(x => !string.IsNullOrEmpty(x.Emoji));

        RuleFor(x => x.Colors)
            .MaximumLength(2000).WithMessage("Colors слишком длинный")
            .Must(colors => string.IsNullOrEmpty(colors) || IsValidJson(colors)).WithMessage("Colors должен быть корректным JSON")
            .When(x => x.Colors != null);

        static bool IsValidJson(string json)
        {
            try
            {
                System.Text.Json.JsonDocument.Parse(json);
                return true;
            }
            catch
            {
                return false;
            }
        }

    }
}

public class UpdateThemeCommandHandler(IThemeRepository repository) : IRequestHandler<UpdateThemeCommand, UpdateThemeResult>
{
    private readonly IThemeRepository _repository = repository;

    public async Task<UpdateThemeResult> Handle(UpdateThemeCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var themeId = Guid.Parse(request.ThemeId);

            var existing = await _repository.GetByIdAsync(themeId);
            if (existing == null)
                return UpdateThemeResult.ThemeNotFound();

            var theme = Theme.Update(
                existingTheme: existing,
                name: request.Name,
                emoji: request.Emoji,
                colors: request.Colors);

            var updated = await _repository.UpdateAsync(theme);

            if (updated == null)
                return UpdateThemeResult.UpdateFailed();

            return UpdateThemeResult.UpdateSuccess(updated);
        }
        catch (Exception ex)
        {
            throw new CqrsResultException(UpdateThemeResult.UpdateFailed(), ex);
        }
    }
}
