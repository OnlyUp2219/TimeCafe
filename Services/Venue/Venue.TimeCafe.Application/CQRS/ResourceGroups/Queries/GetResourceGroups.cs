namespace Venue.TimeCafe.Application.CQRS.ResourceGroups.Queries;

public record GetResourceGroupsQuery() : IQuery<IEnumerable<ResourceGroupDto>>;

public class GetResourceGroupsQueryHandler(IUnitOfWork uow) : IQueryHandler<GetResourceGroupsQuery, IEnumerable<ResourceGroupDto>>
{
    private readonly IUnitOfWork _uow = uow;

    public async Task<Result<IEnumerable<ResourceGroupDto>>> Handle(GetResourceGroupsQuery request, CancellationToken cancellationToken)
    {
        var groups = await _uow.ResourceGroups.GetAllAsync(cancellationToken);
        var dtos = groups.Select(rg => new ResourceGroupDto(rg.ResourceGroupId, rg.Name, rg.Description, rg.Capacity, rg.IsActive));
        return Result.Ok(dtos);
    }
}
