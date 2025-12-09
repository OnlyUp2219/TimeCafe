namespace UserProfile.TimeCafe.Application.CQRS.AdditionalInfos.Commands;

public record UpdateAdditionalInfoCommand(string InfoId, string UserId, string InfoText, string? CreatedBy = null) : IRequest<UpdateAdditionalInfoResult>;

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
        RuleFor(x => x.InfoId)
            .NotEmpty().WithMessage("Информации отсутствует")
            .Must(x => !string.IsNullOrWhiteSpace(x)).WithMessage("Информации отсутствует")
            .Must(x => Guid.TryParse(x, out _)).WithMessage("Информации отсутствует");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Такого пользователя не существует")
            .Must(x => !string.IsNullOrWhiteSpace(x)).WithMessage("Такого пользователя не существует")
            .Must(x => Guid.TryParse(x, out _)).WithMessage("Такого пользователя не существует");

        RuleFor(x => x.InfoText)
            .NotEmpty().WithMessage("Текст информации обязателен")
            .MaximumLength(2000).WithMessage("Текст не может превышать 2000 символов");

        RuleFor(x => x.CreatedBy)
            .MaximumLength(450).WithMessage("Создано кем - не может превышать 450 символов");
    }
}

public class UpdateAdditionalInfoCommandHandler(IAdditionalInfoRepository repository) : IRequestHandler<UpdateAdditionalInfoCommand, UpdateAdditionalInfoResult>
{
    private readonly IAdditionalInfoRepository _repository = repository;

    public async Task<UpdateAdditionalInfoResult> Handle(UpdateAdditionalInfoCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var userId = Guid.Parse(request.UserId);
            var infoId = Guid.Parse(request.InfoId);

            var existing = await _repository.GetAdditionalInfoByIdAsync(infoId, cancellationToken);
            if (existing == null)
                return UpdateAdditionalInfoResult.InfoNotFound();

            //TODO : Mapping and updating CreatedBy
            var additionalInfo = new AdditionalInfo
            {
                InfoId = infoId,
                UserId = userId,
                InfoText = request.InfoText,
                CreatedAt = existing.CreatedAt,
                CreatedBy =  request.CreatedBy,
            };

            var updated = await _repository.UpdateAdditionalInfoAsync(additionalInfo, cancellationToken);

            if (updated == null)
                return UpdateAdditionalInfoResult.UpdateFailed();

            return UpdateAdditionalInfoResult.UpdateSuccess(updated);
        }
        catch (Exception)
        {
            return UpdateAdditionalInfoResult.UpdateFailed();
        }
    }
}
