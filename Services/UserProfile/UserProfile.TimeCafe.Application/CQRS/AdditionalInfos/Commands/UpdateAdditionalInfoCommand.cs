namespace UserProfile.TimeCafe.Application.CQRS.AdditionalInfos.Commands;

public record UpdateAdditionalInfoCommand(Guid InfoId, Guid UserId, string InfoText, string? CreatedBy = null) : IRequest<UpdateAdditionalInfoResult>;

public record UpdateAdditionalInfoResult(
    bool Success,
    string? Code = null,
    string? Message = null,
    int? StatusCode = null,
    List<ErrorItem>? Errors = null,
    AdditionalInfo? AdditionalInfo = null) : ICqrsResultV2
{
    public static UpdateAdditionalInfoResult InfoNotFound() =>
        new(false, Code: "AdditionalInfoNotFound", Message: "Дополнительная информация не найдена", StatusCode: 404);

    public static UpdateAdditionalInfoResult UpdateFailed() =>
        new(false, Code: "UpdateAdditionalInfoFailed", Message: "Не удалось обновить дополнительную информацию", StatusCode: 500);

    public static UpdateAdditionalInfoResult UpdateSuccess(AdditionalInfo info) =>
        new(true, Message: "Дополнительная информация успешно обновлена", AdditionalInfo: info);
}

public class UpdateAdditionalInfoCommandValidator : AbstractValidator<UpdateAdditionalInfoCommand>
{
    public UpdateAdditionalInfoCommandValidator()
    {
        RuleFor(x => x.InfoId).ValidGuidEntityId("Информации отсутствует");

        RuleFor(x => x.UserId).ValidGuidEntityId("Такого пользователя не существует");

        RuleFor(x => x.InfoText).ValidInfoText();

        RuleFor(x => x.CreatedBy).ValidCreatedBy();
    }
}

public class UpdateAdditionalInfoCommandHandler(IAdditionalInfoRepository repository) : IRequestHandler<UpdateAdditionalInfoCommand, UpdateAdditionalInfoResult>
{
    private readonly IAdditionalInfoRepository _repository = repository;

    public async Task<UpdateAdditionalInfoResult> Handle(UpdateAdditionalInfoCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var existing = await _repository.GetAdditionalInfoByIdAsync(request.InfoId, cancellationToken);
            if (existing == null)
                return UpdateAdditionalInfoResult.InfoNotFound();

            //TODO : Mapping and updating CreatedBy
            var additionalInfo = new AdditionalInfo
            {
                InfoId = request.InfoId,
                UserId = request.UserId,
                InfoText = request.InfoText,
                CreatedAt = existing.CreatedAt,
                CreatedBy = request.CreatedBy,
            };

            var updated = await _repository.UpdateAdditionalInfoAsync(additionalInfo, cancellationToken);

            if (updated == null)
                return UpdateAdditionalInfoResult.UpdateFailed();

            return UpdateAdditionalInfoResult.UpdateSuccess(updated);
        }
        catch (Exception ex)
        {
            throw new CqrsResultException(UpdateAdditionalInfoResult.UpdateFailed(), ex);
        }
    }
}
