namespace UserProfile.TimeCafe.Application.CQRS.AdditionalInfos.Queries;

public record GetAdditionalInfosByUserIdQuery(Guid UserId, int PageNumber, int PageSize) : IQuery<IEnumerable<AdditionalInfo>>;

public class GetAdditionalInfosByUserIdQueryHandler(IUnitOfWork uow) : IQueryHandler<GetAdditionalInfosByUserIdQuery, IEnumerable<AdditionalInfo>>
{
    public async Task<Result<IEnumerable<AdditionalInfo>>> Handle(GetAdditionalInfosByUserIdQuery request, CancellationToken cancellationToken = default)
    {
        try
        {
            var infos = await uow.AdditionalInfos.GetByUserIdAsync(request.UserId, cancellationToken);
            return Result.Ok(infos);
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error("Внутренняя ошибка").CausedBy(ex));
        }
    }
}
