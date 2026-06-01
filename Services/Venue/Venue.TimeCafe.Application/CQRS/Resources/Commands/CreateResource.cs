namespace Venue.TimeCafe.Application.CQRS.Resources.Commands;

public record CreateResourceCommand(Guid ResourceGroupId, string Name, int Capacity, bool IsActive = true) : ICommand<ResourceDto>;

public class CreateResourceCommandValidator : AbstractValidator<CreateResourceCommand>
{
    public CreateResourceCommandValidator()
    {
        RuleFor(x => x.ResourceGroupId).NotEmpty().WithMessage("Идентификатор группы столов обязателен");
        RuleFor(x => x.Name).NotEmpty().WithMessage("Имя стола обязательно");
        RuleFor(x => x.Capacity).GreaterThan(0).WithMessage("Вместимость стола должна быть больше 0");
    }
}

public class CreateResourceCommandHandler(
    IUnitOfWork uow,
    IPublisher publisher) : ICommandHandler<CreateResourceCommand, ResourceDto>
{
    private readonly IUnitOfWork _uow = uow;
    private readonly IPublisher _publisher = publisher;

    public async Task<Result<ResourceDto>> Handle(CreateResourceCommand request, CancellationToken cancellationToken)
    {
        var group = await _uow.ResourceGroups.GetByIdAsync(request.ResourceGroupId, cancellationToken);
        if (group == null) return Result.Fail<ResourceDto>(new ResourceGroupNotFoundError());

        var resource = Resource.Create(
            id: null,
            resourceGroupId: request.ResourceGroupId,
            name: request.Name,
            capacity: request.Capacity,
            isActive: request.IsActive
        );

        var created = await _uow.Resources.CreateAsync(resource, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        await _publisher.Publish(new ResourceChangedEvent(created.ResourceId), cancellationToken);

        return Result.Ok(new ResourceDto(created.ResourceId, created.ResourceGroupId, created.Name, created.Capacity, created.IsActive));
    }
}
