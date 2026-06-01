namespace Venue.TimeCafe.Application.CQRS.Resources.Commands;

public record UpdateResourceCommand(Guid ResourceId, Guid ResourceGroupId, string Name, int Capacity, bool IsActive) : ICommand<ResourceDto>;

public class UpdateResourceCommandValidator : AbstractValidator<UpdateResourceCommand>
{
    public UpdateResourceCommandValidator()
    {
        RuleFor(x => x.ResourceId).NotEmpty().WithMessage("Идентификатор стола обязателен");
        RuleFor(x => x.ResourceGroupId).NotEmpty().WithMessage("Идентификатор группы столов обязателен");
        RuleFor(x => x.Name).NotEmpty().WithMessage("Имя стола обязательно");
        RuleFor(x => x.Capacity).GreaterThan(0).WithMessage("Вместимость стола должна быть больше 0");
    }
}

public class UpdateResourceCommandHandler(
    IUnitOfWork uow,
    IPublisher publisher) : ICommandHandler<UpdateResourceCommand, ResourceDto>
{
    private readonly IUnitOfWork _uow = uow;
    private readonly IPublisher _publisher = publisher;

    public async Task<Result<ResourceDto>> Handle(UpdateResourceCommand request, CancellationToken cancellationToken)
    {
        var existing = await _uow.Resources.GetByIdAsync(request.ResourceId, cancellationToken);
        if (existing == null) return Result.Fail<ResourceDto>(new ResourceNotFoundError());

        var group = await _uow.ResourceGroups.GetByIdAsync(request.ResourceGroupId, cancellationToken);
        if (group == null) return Result.Fail<ResourceDto>(new ResourceGroupNotFoundError());

        existing.ResourceGroupId = request.ResourceGroupId;
        existing.Name = request.Name;
        existing.Capacity = request.Capacity;
        existing.IsActive = request.IsActive;

        await _uow.Resources.UpdateAsync(existing, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        await _publisher.Publish(new ResourceChangedEvent(existing.ResourceId), cancellationToken);

        return Result.Ok(new ResourceDto(existing.ResourceId, existing.ResourceGroupId, existing.Name, existing.Capacity, existing.IsActive));
    }
}
