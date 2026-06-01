namespace Venue.TimeCafe.Application.CQRS.Resources.Queries;

public record GetResourceByIdQuery(Guid ResourceId) : IQuery<ResourceDto>;

public class GetResourceByIdQueryHandler(IUnitOfWork uow) : IQueryHandler<GetResourceByIdQuery, ResourceDto>
{
    private readonly IUnitOfWork _uow = uow;

    public async Task<Result<ResourceDto>> Handle(GetResourceByIdQuery request, CancellationToken cancellationToken)
    {
        var r = await _uow.Resources.GetByIdAsync(request.ResourceId, cancellationToken);
        if (r == null) return Result.Fail<ResourceDto>(new ResourceNotFoundError());
        return Result.Ok(new ResourceDto(r.ResourceId, r.ResourceGroupId, r.Name, r.Capacity, r.IsActive));
    }
}
