namespace UserProfile.TimeCafe.Application.CQRS.AdditionalInfos.Queries;

public record GetAdditionalInfoByIdQuery(Guid Id) : IQuery<AdditionalInfo>;

public class GetAdditionalInfoByIdQueryHandler(IUnitOfWork uow) : IQueryHandler<GetAdditionalInfoByIdQuery, AdditionalInfo>
{
    public async Task<Result<AdditionalInfo>> Handle(GetAdditionalInfoByIdQuery request, CancellationToken cancellationToken = default)
    {
        try
        {
            var info = await uow.AdditionalInfos.GetByIdAsync(request.Id, cancellationToken);
            return info != null ? Result.Ok(info) : Result.Fail(new InfoNotFoundError());
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error("Внутренняя ошибка").CausedBy(ex));
        }
    }
}
