namespace Venue.TimeCafe.Application.CQRS.Resources.Queries;

public record GetResourcesQuery() : IQuery<IEnumerable<ResourceDto>>;

public class GetResourcesQueryHandler(IUnitOfWork uow) : IQueryHandler<GetResourcesQuery, IEnumerable<ResourceDto>>
{
    private readonly IUnitOfWork _uow = uow;

    public async Task<Result<IEnumerable<ResourceDto>>> Handle(GetResourcesQuery request, CancellationToken cancellationToken)
    {
        var resources = await _uow.Resources.GetAllAsync(cancellationToken);
        var dtos = resources.Select(r => new ResourceDto(r.ResourceId, r.ResourceGroupId, r.Name, r.Capacity, r.IsActive));
        return Result.Ok(dtos);
    }
}
