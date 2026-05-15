namespace UserProfile.TimeCafe.Application.CQRS.AdditionalInfos.Queries;

public record GetAdditionalInfosByUserIdQuery(Guid UserId, int Page, int PageSize) : IQuery<PagedResponse<AdditionalInfoDto>>;

public record AdditionalInfoDto(
    Guid InfoId,
    Guid UserId,
    string InfoText,
    string CreatedBy,
    DateTimeOffset CreatedAt);

public class GetAdditionalInfosByUserIdQueryHandler(IUnitOfWork uow) : IQueryHandler<GetAdditionalInfosByUserIdQuery, PagedResponse<AdditionalInfoDto>>
{
    private readonly IUnitOfWork _uow = uow;

    public async Task<Result<PagedResponse<AdditionalInfoDto>>> Handle(GetAdditionalInfosByUserIdQuery request, CancellationToken cancellationToken = default)
    {
        try
        {
            var infos = await _uow.AdditionalInfos.GetByUserIdAsync(request.UserId, cancellationToken);
            
            var totalCount = infos.Count();
            var totalPages = (totalCount + request.PageSize - 1) / request.PageSize;

            var dtos = infos.Select(i => new AdditionalInfoDto(
                i.InfoId,
                i.UserId,
                i.InfoText,
                i.CreatedBy,
                i.CreatedAt));

            return Result.Ok(new PagedResponse<AdditionalInfoDto>(
                dtos,
                new PageMetadata(request.Page, request.PageSize, totalCount, totalPages)));
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error("Внутренняя ошибка").CausedBy(ex));
        }
    }
}
