namespace Venue.TimeCafe.Application.CQRS.ResourceGroups.Queries;

public record GetResourceGroupByIdQuery(Guid ResourceGroupId) : IQuery<ResourceGroupDto>;

public class GetResourceGroupByIdQueryHandler(IUnitOfWork uow) : IQueryHandler<GetResourceGroupByIdQuery, ResourceGroupDto>
{
    private readonly IUnitOfWork _uow = uow;

    public async Task<Result<ResourceGroupDto>> Handle(GetResourceGroupByIdQuery request, CancellationToken cancellationToken)
    {
        var rg = await _uow.ResourceGroups.GetByIdAsync(request.ResourceGroupId, cancellationToken);
        if (rg == null) return Result.Fail<ResourceGroupDto>(new ResourceGroupNotFoundError());
        return Result.Ok(new ResourceGroupDto(rg.ResourceGroupId, rg.Name, rg.Description, rg.Capacity, rg.IsActive));
    }
}
