namespace Venue.TimeCafe.Application.CQRS.ResourceGroups.Commands;

public record CreateResourceGroupCommand(string Name, string? Description, int Capacity, bool IsActive = true) : ICommand<ResourceGroupDto>;

public class CreateResourceGroupCommandValidator : AbstractValidator<CreateResourceGroupCommand>
{
    public CreateResourceGroupCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Имя группы обязательно");
        RuleFor(x => x.Capacity).GreaterThan(0).WithMessage("Вместимость группы должна быть больше 0");
    }
}

public class CreateResourceGroupCommandHandler(
    IUnitOfWork uow,
    IPublisher publisher) : ICommandHandler<CreateResourceGroupCommand, ResourceGroupDto>
{
    private readonly IUnitOfWork _uow = uow;
    private readonly IPublisher _publisher = publisher;

    public async Task<Result<ResourceGroupDto>> Handle(CreateResourceGroupCommand request, CancellationToken cancellationToken)
    {
        var group = ResourceGroup.Create(
            id: null,
            name: request.Name,
            description: request.Description,
            capacity: request.Capacity,
            isActive: request.IsActive
        );

        var created = await _uow.ResourceGroups.CreateAsync(group, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        await _publisher.Publish(new ResourceGroupChangedEvent(created.ResourceGroupId), cancellationToken);

        return Result.Ok(new ResourceGroupDto(created.ResourceGroupId, created.Name, created.Description, created.Capacity, created.IsActive));
    }
}
