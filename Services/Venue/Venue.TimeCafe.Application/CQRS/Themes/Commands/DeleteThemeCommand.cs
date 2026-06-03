namespace Venue.TimeCafe.Application.CQRS.Themes.Commands;

public record DeleteThemeCommand(Guid ThemeId) : ICommand;

public class DeleteThemeCommandValidator : AbstractValidator<DeleteThemeCommand>
{
    public DeleteThemeCommandValidator()
    {
        RuleFor(x => x.ThemeId).ValidGuidEntityId("Тема не найдена");
    }
}

public class DeleteThemeCommandHandler(IUnitOfWork uow, IPublisher publisher) : ICommandHandler<DeleteThemeCommand>
{
    private readonly IUnitOfWork _uow = uow;
    private readonly IPublisher _publisher = publisher;

    public async Task<Result> Handle(DeleteThemeCommand request, CancellationToken cancellationToken = default)
    {
        try
        {
            var existing = await _uow.Themes.GetByIdAsync(request.ThemeId, cancellationToken);
            if (existing == null)
                return Result.Fail(new ThemeNotFoundError());

            var hasTariffs = await _uow.Tariffs.AnyWithThemeIdAsync(request.ThemeId, cancellationToken);
            if (hasTariffs)
                return Result.Fail(new ThemeInUseError());

            var result = await _uow.Themes.DeleteAsync(request.ThemeId, cancellationToken);
            if (!result)
                return Result.Fail(new ThemeDeleteFailedError());

            await _uow.SaveChangesAsync(cancellationToken);
            await _publisher.Publish(new ThemeChangedEvent(request.ThemeId), cancellationToken);

            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error("Внутренняя ошибка").CausedBy(ex));
        }
    }
}

