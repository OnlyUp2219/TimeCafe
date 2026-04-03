namespace Venue.TimeCafe.Application.CQRS.Themes.Commands;

public record DeleteThemeCommand(Guid ThemeId) : IRequest<DeleteThemeResult>;

public record DeleteThemeResult(
    bool Success,
    string? Code = null,
    string? Message = null,
    int? StatusCode = null,
    List<ErrorItem>? Errors = null) : ICqrsResult
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
        RuleFor(x => x.ThemeId).ValidGuidEntityId("Тема не найдена");
    }
}

public class DeleteThemeCommandHandler(IThemeRepository repository) : IRequestHandler<DeleteThemeCommand, DeleteThemeResult>
{
    private readonly IThemeRepository _repository = repository;

    public async Task<DeleteThemeResult> Handle(DeleteThemeCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var existing = await _repository.GetByIdAsync(request.ThemeId);
            if (existing == null)
                return DeleteThemeResult.ThemeNotFound();

            var result = await _repository.DeleteAsync(request.ThemeId);

            if (!result)
                return DeleteThemeResult.DeleteFailed();

            return DeleteThemeResult.DeleteSuccess();
        }
        catch (Exception ex)
        {
            throw new CqrsResultException(DeleteThemeResult.DeleteFailed(), ex);
        }
    }
}
