namespace UserProfile.TimeCafe.Application.CQRS.AdditionalInfos.Queries;

public record GetAdditionalInfosByUserIdQuery(Guid UserId) : IRequest<GetAdditionalInfosByUserIdResult>;

public record GetAdditionalInfosByUserIdResult(
    bool Success,
    string? Code = null,
    string? Message = null,
    int? StatusCode = null,
    List<ErrorItem>? Errors = null,
    IEnumerable<AdditionalInfo>? AdditionalInfos = null) : ICqrsResultV2
{
    public static GetAdditionalInfosByUserIdResult GetFailed() =>
        new(false, Code: "GetAdditionalInfosFailed", Message: "Не удалось получить дополнительную информацию", StatusCode: 500);

    public static GetAdditionalInfosByUserIdResult GetSuccess(IEnumerable<AdditionalInfo> infos) =>
        new(true, Message: "Дополнительная информация получена", AdditionalInfos: infos);
}

public class GetAdditionalInfosByUserIdQueryValidator : AbstractValidator<GetAdditionalInfosByUserIdQuery>
{
    public GetAdditionalInfosByUserIdQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId обязателен");
    }
}

public class GetAdditionalInfosByUserIdQueryHandler(IAdditionalInfoRepository repository) : IRequestHandler<GetAdditionalInfosByUserIdQuery, GetAdditionalInfosByUserIdResult>
{
    private readonly IAdditionalInfoRepository _repository = repository;

    public async Task<GetAdditionalInfosByUserIdResult> Handle(GetAdditionalInfosByUserIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var infos = await _repository.GetAdditionalInfosByUserIdAsync(request.UserId, cancellationToken);

            return GetAdditionalInfosByUserIdResult.GetSuccess(infos);
        }
        catch (Exception)
        {
            return GetAdditionalInfosByUserIdResult.GetFailed();
        }
    }
}
