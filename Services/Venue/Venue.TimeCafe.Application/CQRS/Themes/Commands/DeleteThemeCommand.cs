namespace Venue.TimeCafe.Application.CQRS.Themes.Commands;

public record DeleteThemeCommand(int ThemeId) : IRequest<DeleteThemeResult>;

public record DeleteThemeResult(
    bool Success,
    string? Code = null,
    string? Message = null,
    int? StatusCode = null,
    List<ErrorItem>? Errors = null) : ICqrsResultV2
{
    public static DeleteThemeResult ThemeNotFound() =>
        new(false, Code: "ThemeNotFound", Message: "Тема не найдена", StatusCode: 404);

    public static DeleteThemeResult DeleteFailed() =>
        new(false, Code: "DeleteThemeFailed", Message: "Не удалось удалить тему", StatusCode: 500);

    public static DeleteThemeResult DeleteSuccess() =>
        new(true, Message: "Тема успешно удалена");
}

public class DeleteThemeCommandValidator : AbstractValidator<DeleteThemeCommand>
{
    public DeleteThemeCommandValidator()
    {
        RuleFor(x => x.ThemeId)
            .GreaterThan(0).WithMessage("ID темы обязателен");
    }
}

public class DeleteThemeCommandHandler(IThemeRepository repository) : IRequestHandler<DeleteThemeCommand, DeleteThemeResult>
{
    private readonly IThemeRepository _repository = repository;

    public async Task<DeleteThemeResult> Handle(DeleteThemeCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _repository.DeleteAsync(request.ThemeId);

            if (!result)
                return DeleteThemeResult.ThemeNotFound();

            return DeleteThemeResult.DeleteSuccess();
        }
        catch (Exception)
        {
            return DeleteThemeResult.DeleteFailed();
        }
    }
}
