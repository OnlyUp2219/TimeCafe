namespace Venue.TimeCafe.Application.CQRS.Themes.Commands;

public record UpdateThemeCommand(Theme Theme) : IRequest<UpdateThemeResult>;

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
        RuleFor(x => x.Theme)
            .NotNull().WithMessage("Тема обязательна");

        When(x => x.Theme != null, () =>
        {
            RuleFor(x => x.Theme.ThemeId)
                .GreaterThan(0).WithMessage("ID темы обязателен");

            RuleFor(x => x.Theme.Name)
                .NotEmpty().WithMessage("Название темы обязательно")
                .MaximumLength(100).WithMessage("Название не может превышать 100 символов");

            RuleFor(x => x.Theme.Emoji)
                .MaximumLength(10).WithMessage("Эмодзи не может превышать 10 символов")
                .When(x => !string.IsNullOrEmpty(x.Theme.Emoji));
        });
    }
}

public class UpdateThemeCommandHandler(IThemeRepository repository) : IRequestHandler<UpdateThemeCommand, UpdateThemeResult>
{
    private readonly IThemeRepository _repository = repository;

    public async Task<UpdateThemeResult> Handle(UpdateThemeCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var existing = await _repository.GetByIdAsync(request.Theme.ThemeId);
            if (existing == null)
                return UpdateThemeResult.ThemeNotFound();

            var updated = await _repository.UpdateAsync(request.Theme);

            if (updated == null)
                return UpdateThemeResult.UpdateFailed();

            return UpdateThemeResult.UpdateSuccess(updated);
        }
        catch (Exception)
        {
            return UpdateThemeResult.UpdateFailed();
        }
    }
}
