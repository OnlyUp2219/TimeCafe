namespace Venue.TimeCafe.Application.CQRS.Themes.Commands;

public record CreateThemeCommand(string Name, string? Emoji, string? Colors) : ICommand<Theme>;

public class CreateThemeCommandValidator : AbstractValidator<CreateThemeCommand>
{
    public CreateThemeCommandValidator()
    {
        RuleFor(x => x.Name).ValidName("Название темы");

        RuleFor(x => x.Emoji).ValidEmoji()
            .When(x => !string.IsNullOrEmpty(x.Emoji));
    }
}

public class CreateThemeCommandHandler(IUnitOfWork uow, IPublisher publisher) : ICommandHandler<CreateThemeCommand, Theme>
{
    private readonly IUnitOfWork _uow = uow;
    private readonly IPublisher _publisher = publisher;

    public async Task<Result<Theme>> Handle(CreateThemeCommand request, CancellationToken cancellationToken = default)
    {
        try
        {
            var theme = new Theme
            {
                Name = request.Name,
                Emoji = request.Emoji,
                Colors = request.Colors
            };

            var created = await _uow.Themes.CreateAsync(theme, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);

            if (created == null)
                return Result.Fail(new CreateFailedError());

            await _publisher.Publish(new ThemeChangedEvent(created.ThemeId), cancellationToken);

            return Result.Ok(created);
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error("Внутренняя ошибка").CausedBy(ex));
        }
    }
}

