namespace Venue.TimeCafe.Application.CQRS.Themes.Commands;

public record CreateThemeCommand(string Name, string? Emoji, string? Colors) : IRequest<CreateThemeResult>;

public record CreateThemeResult(
    bool Success,
    string? Code = null,
    string? Message = null,
    int? StatusCode = null,
    List<ErrorItem>? Errors = null,
    Theme? Theme = null) : ICqrsResultV2
{
    public static CreateThemeResult CreateFailed() =>
        new(false, Code: "CreateThemeFailed", Message: "Не удалось создать тему", StatusCode: 500);

    public static CreateThemeResult CreateSuccess(Theme theme) =>
        new(true, Message: "Тема успешно создана", StatusCode: 201, Theme: theme);
}

public class CreateThemeCommandValidator : AbstractValidator<CreateThemeCommand>
{
    public CreateThemeCommandValidator()
    {
        RuleFor(x => x.Name).ValidName("Название темы");

        RuleFor(x => x.Emoji).ValidEmoji()
            .When(x => !string.IsNullOrEmpty(x.Emoji));
    }
}

public class CreateThemeCommandHandler(IThemeRepository repository) : IRequestHandler<CreateThemeCommand, CreateThemeResult>
{
    private readonly IThemeRepository _repository = repository;

    public async Task<CreateThemeResult> Handle(CreateThemeCommand request, CancellationToken cancellationToken)
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
                return CreateThemeResult.CreateFailed();

            return CreateThemeResult.CreateSuccess(created);
        }
        catch (Exception ex)
        {
            throw new CqrsResultException(CreateThemeResult.CreateFailed(), ex);
        }
    }
}
