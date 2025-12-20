namespace Venue.TimeCafe.Application.CQRS.Themes.Commands;

public record DeleteThemeCommand(string ThemeId) : IRequest<DeleteThemeResult>;

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
            .NotEmpty().WithMessage("Тема не найдена")
           .NotNull().WithMessage("Тема не найдена")
            .Must(x => Guid.TryParse(x, out var guid) && guid != Guid.Empty).WithMessage("Тема не найдена");
    }
}

public class DeleteThemeCommandHandler(IThemeRepository repository) : IRequestHandler<DeleteThemeCommand, DeleteThemeResult>
{
    private readonly IThemeRepository _repository = repository;

    public async Task<DeleteThemeResult> Handle(DeleteThemeCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var themeId = Guid.Parse(request.ThemeId);

            var existing = await _repository.GetByIdAsync(themeId);
            if (existing == null)
                return DeleteThemeResult.ThemeNotFound();

            var result = await _repository.DeleteAsync(themeId);

            if (!result)
                return DeleteThemeResult.DeleteFailed();

            return DeleteThemeResult.DeleteSuccess();
        }
        catch (Exception)
        {
            return DeleteThemeResult.DeleteFailed();
        }
    }
}
