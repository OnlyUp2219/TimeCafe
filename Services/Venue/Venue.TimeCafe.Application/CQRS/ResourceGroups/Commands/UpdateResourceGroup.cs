namespace Venue.TimeCafe.Application.CQRS.ResourceGroups.Commands;

public record UpdateResourceGroupCommand(Guid ResourceGroupId, string Name, string? Description, int Capacity, bool IsActive) : ICommand<ResourceGroupDto>;

public class UpdateResourceGroupCommandValidator : AbstractValidator<UpdateResourceGroupCommand>
{
    public UpdateResourceGroupCommandValidator()
    {
        RuleFor(x => x.ResourceGroupId).NotEmpty().WithMessage("Идентификатор группы обязателен");
        RuleFor(x => x.Name).NotEmpty().WithMessage("Имя группы обязательно");
        RuleFor(x => x.Capacity).GreaterThan(0).WithMessage("Вместимость группы должна быть больше 0");
    }
}

public class UpdateResourceGroupCommandHandler(
    IUnitOfWork uow,
    IPublisher publisher) : ICommandHandler<UpdateResourceGroupCommand, ResourceGroupDto>
{
    private readonly IUnitOfWork _uow = uow;
    private readonly IPublisher _publisher = publisher;

    public async Task<Result<ResourceGroupDto>> Handle(UpdateResourceGroupCommand request, CancellationToken cancellationToken)
    {
        var existing = await _uow.ResourceGroups.GetByIdAsync(request.ResourceGroupId, cancellationToken);
        if (existing == null) return Result.Fail<ResourceGroupDto>(new ResourceGroupNotFoundError());

        existing.Name = request.Name;
        existing.Description = request.Description;
        existing.Capacity = request.Capacity;
        existing.IsActive = request.IsActive;

        await _uow.ResourceGroups.UpdateAsync(existing, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        await _publisher.Publish(new ResourceGroupChangedEvent(existing.ResourceGroupId), cancellationToken);

        return Result.Ok(new ResourceGroupDto(existing.ResourceGroupId, existing.Name, existing.Description, existing.Capacity, existing.IsActive));
    }
}
