namespace UserProfile.TimeCafe.Application.CQRS.AdditionalInfos.Queries;

public record GetAdditionalInfoByIdQuery(int InfoId) : IRequest<GetAdditionalInfoByIdResult>;

public record GetAdditionalInfoByIdResult(
    bool Success,
    string? Code = null,
    string? Message = null,
    int? StatusCode = null,
    List<ErrorItem>? Errors = null,
    AdditionalInfo? AdditionalInfo = null) : ICqrsResultV2
{
    public static GetAdditionalInfoByIdResult InfoNotFound() =>
        new(false, Code: "AdditionalInfoNotFound", Message: "Дополнительная информация не найдена", StatusCode: 404);

    public static GetAdditionalInfoByIdResult GetFailed() =>
        new(false, Code: "GetAdditionalInfoFailed", Message: "Не удалось получить дополнительную информацию", StatusCode: 500);

    public static GetAdditionalInfoByIdResult GetSuccess(AdditionalInfo info) =>
        new(true, Message: "Дополнительная информация найдена", AdditionalInfo: info);
}

public class GetAdditionalInfoByIdQueryValidator : AbstractValidator<GetAdditionalInfoByIdQuery>
{
    public GetAdditionalInfoByIdQueryValidator()
    {
        RuleFor(x => x.InfoId)
            .GreaterThan(0).WithMessage("InfoId должен быть больше 0");
    }
}

public class GetAdditionalInfoByIdQueryHandler(IAdditionalInfoRepository repository) : IRequestHandler<GetAdditionalInfoByIdQuery, GetAdditionalInfoByIdResult>
{
    private readonly IAdditionalInfoRepository _repository = repository;

    public async Task<GetAdditionalInfoByIdResult> Handle(GetAdditionalInfoByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var info = await _repository.GetAdditionalInfoByIdAsync(request.InfoId, cancellationToken);

            if (info == null)
                return GetAdditionalInfoByIdResult.InfoNotFound();

            return GetAdditionalInfoByIdResult.GetSuccess(info);
        }
        catch (Exception)
        {
            return GetAdditionalInfoByIdResult.GetFailed();
        }
    }
}
