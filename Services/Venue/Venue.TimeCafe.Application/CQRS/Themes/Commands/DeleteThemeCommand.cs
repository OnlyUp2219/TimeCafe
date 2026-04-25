namespace Venue.TimeCafe.Application.CQRS.Themes.Commands;

public record DeleteThemeCommand(Guid ThemeId) : ICommand;

public class DeleteThemeCommandValidator : AbstractValidator<DeleteThemeCommand>
{
    public DeleteThemeCommandValidator()
    {
        RuleFor(x => x.ThemeId).ValidGuidEntityId("Тема не найдена");
    }
}

public class DeleteThemeCommandHandler(IThemeRepository repository) : ICommandHandler<DeleteThemeCommand>
{
    private readonly IThemeRepository _repository = repository;

    public async Task<Result> Handle(DeleteThemeCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var existing = await _repository.GetByIdAsync(request.ThemeId);
            if (existing == null)
                return Result.Fail(new ThemeNotFoundError());

            var result = await _repository.DeleteAsync(request.ThemeId);

            if (!result)
                return Result.Fail(new DeleteFailedError());

            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error("Внутренняя ошибка").CausedBy(ex));
        }
    }
}

