namespace Venue.TimeCafe.Application.CQRS.ResourceGroups.Commands;

public record DeleteResourceGroupCommand(Guid ResourceGroupId) : ICommand<bool>;

public class DeleteResourceGroupCommandValidator : AbstractValidator<DeleteResourceGroupCommand>
{
    public DeleteResourceGroupCommandValidator()
    {
        RuleFor(x => x.ResourceGroupId).ValidGuidEntityId("Зона не найдена");
    }
}

public class DeleteResourceGroupCommandHandler(
    IUnitOfWork uow,
    IPublisher publisher) : ICommandHandler<DeleteResourceGroupCommand, bool>
{
    private readonly IUnitOfWork _uow = uow;
    private readonly IPublisher _publisher = publisher;

    public async Task<Result<bool>> Handle(DeleteResourceGroupCommand request, CancellationToken cancellationToken)
    {
        var hasResources = await _uow.Resources.AnyWithGroupIdAsync(request.ResourceGroupId, cancellationToken);
        if (hasResources) return Result.Fail<bool>(new ResourceGroupInUseError());

        var deleted = await _uow.ResourceGroups.DeleteAsync(request.ResourceGroupId, cancellationToken);
        if (!deleted) return Result.Fail<bool>(new ResourceGroupNotFoundError());

        await _uow.SaveChangesAsync(cancellationToken);

        await _publisher.Publish(new ResourceGroupChangedEvent(request.ResourceGroupId), cancellationToken);

        return Result.Ok(true);
    }
}
