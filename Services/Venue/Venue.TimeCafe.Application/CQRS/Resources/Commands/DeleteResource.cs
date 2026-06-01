namespace Venue.TimeCafe.Application.CQRS.Resources.Commands;

public record DeleteResourceCommand(Guid ResourceId) : ICommand<bool>;

public class DeleteResourceCommandValidator : AbstractValidator<DeleteResourceCommand>
{
    public DeleteResourceCommandValidator()
    {
        RuleFor(x => x.ResourceId).NotEmpty().WithMessage("Идентификатор стола обязателен");
    }
}

public class DeleteResourceCommandHandler(
    IUnitOfWork uow,
    IPublisher publisher) : ICommandHandler<DeleteResourceCommand, bool>
{
    private readonly IUnitOfWork _uow = uow;
    private readonly IPublisher _publisher = publisher;

    public async Task<Result<bool>> Handle(DeleteResourceCommand request, CancellationToken cancellationToken)
    {
        var deleted = await _uow.Resources.DeleteAsync(request.ResourceId, cancellationToken);
        if (!deleted) return Result.Fail<bool>(new ResourceNotFoundError());

        await _uow.SaveChangesAsync(cancellationToken);

        await _publisher.Publish(new ResourceChangedEvent(request.ResourceId), cancellationToken);

        return Result.Ok(true);
    }
}
