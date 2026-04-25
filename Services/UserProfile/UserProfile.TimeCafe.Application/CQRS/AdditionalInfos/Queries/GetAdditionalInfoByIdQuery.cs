namespace UserProfile.TimeCafe.Application.CQRS.AdditionalInfos.Queries;

public record GetAdditionalInfoByIdQuery(Guid InfoId) : IQuery<AdditionalInfo>;

public class GetAdditionalInfoByIdQueryValidator : AbstractValidator<GetAdditionalInfoByIdQuery>
{
    public GetAdditionalInfoByIdQueryValidator()
    {
        RuleFor(x => x.InfoId).ValidGuidEntityId("Информации отсутствует");
    }
}

public class GetAdditionalInfoByIdQueryHandler(IAdditionalInfoRepository repository) : IQueryHandler<GetAdditionalInfoByIdQuery, AdditionalInfo>
{
    private readonly IAdditionalInfoRepository _repository = repository;

    public async Task<Result<AdditionalInfo>> Handle(GetAdditionalInfoByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var info = await _repository.GetAdditionalInfoByIdAsync(request.InfoId, cancellationToken);

            if (info == null)
                return Result.Fail(new InfoNotFoundError());

            return Result.Ok(info);
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error("Внутренняя ошибка").CausedBy(ex));
        }
    }
}
