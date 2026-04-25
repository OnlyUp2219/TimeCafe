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

public class CreateThemeCommandHandler(IThemeRepository repository) : ICommandHandler<CreateThemeCommand, Theme>
{
    private readonly IThemeRepository _repository = repository;

    public async Task<Result<Theme>> Handle(CreateThemeCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var theme = new Theme
            {
                Name = request.Name,
                Emoji = request.Emoji,
                Colors = request.Colors
            };

            var created = await _repository.CreateAsync(theme);

            if (created == null)
                return Result.Fail(new CreateFailedError());

            return Result.Ok(created);
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error("Внутренняя ошибка").CausedBy(ex));
        }
    }
}

