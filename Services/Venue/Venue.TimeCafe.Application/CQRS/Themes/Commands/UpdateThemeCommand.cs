namespace Venue.TimeCafe.Application.CQRS.Themes.Commands;

public record UpdateThemeCommand(Guid ThemeId, string Name, string? Emoji, string? Colors) : ICommand<Theme>;

public class UpdateThemeCommandValidator : AbstractValidator<UpdateThemeCommand>
{
    public UpdateThemeCommandValidator()
    {
        // TODO : finish up validators
        RuleFor(x => x.ThemeId).ValidGuidEntityId("Тема не найдена");

        RuleFor(x => x.Name).ValidName("Название темы");

        RuleFor(x => x.Emoji).ValidEmoji()
            .When(x => !string.IsNullOrEmpty(x.Emoji));

        RuleFor(x => x.Colors)
            .MaximumLength(10000).WithMessage("Colors слишком длинный")
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

public class UpdateThemeCommandHandler(IUnitOfWork uow, IPublisher publisher) : ICommandHandler<UpdateThemeCommand, Theme>
{
    private readonly IUnitOfWork _uow = uow;
    private readonly IPublisher _publisher = publisher;

    public async Task<Result<Theme>> Handle(UpdateThemeCommand request, CancellationToken cancellationToken = default)
    {
        try
        {
            var existing = await _uow.Themes.GetByIdAsync(request.ThemeId, cancellationToken);
            if (existing == null)
                return Result.Fail(new ThemeNotFoundError());

            var theme = Theme.Update(
                existingTheme: existing,
                name: request.Name,
                emoji: request.Emoji,
                colors: request.Colors);

            var updated = await _uow.Themes.UpdateAsync(theme, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);

            if (updated == null)
                return Result.Fail(new ThemeUpdateFailedError());

            await _publisher.Publish(new ThemeChangedEvent(updated.ThemeId), cancellationToken);

            return Result.Ok(updated);
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error("Внутренняя ошибка").CausedBy(ex));
        }
    }
}

