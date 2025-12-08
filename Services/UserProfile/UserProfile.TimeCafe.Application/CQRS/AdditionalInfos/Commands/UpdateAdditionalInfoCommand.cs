namespace UserProfile.TimeCafe.Application.CQRS.AdditionalInfos.Commands;

public record UpdateAdditionalInfoCommand(AdditionalInfo AdditionalInfo) : IRequest<UpdateAdditionalInfoResult>;

public record UpdateAdditionalInfoResult(
    bool Success,
    string? Code = null,
    string? Message = null,
    int? StatusCode = null,
    List<ErrorItem>? Errors = null,
    AdditionalInfo? AdditionalInfo = null) : ICqrsResultV2
{
    public static UpdateAdditionalInfoResult InfoNotFound() =>
        new(false, Code: "NotFound", Message: "Дополнительная информация не найдена", StatusCode: 404);

    public static UpdateAdditionalInfoResult UpdateFailed() =>
        new(false, Code: "UpdateAdditionalInfoFailed", Message: "Не удалось обновить дополнительную информацию", StatusCode: 500);

    public static UpdateAdditionalInfoResult UpdateSuccess(AdditionalInfo info) =>
        new(true, Message: "Дополнительная информация успешно обновлена", AdditionalInfo: info);
}

public class UpdateAdditionalInfoCommandValidator : AbstractValidator<UpdateAdditionalInfoCommand>
{
    public UpdateAdditionalInfoCommandValidator()
    {
        RuleFor(x => x.AdditionalInfo)
            .NotNull().WithMessage("Дополнительная информация обязательна");

        RuleFor(x => x.AdditionalInfo.InfoId)
            .NotEmpty().WithMessage("InfoId обязателен")
            .When(x => x.AdditionalInfo != null);

        RuleFor(x => x.AdditionalInfo.UserId)
            .NotEmpty().WithMessage("UserId обязателен")
            .When(x => x.AdditionalInfo != null);

        RuleFor(x => x.AdditionalInfo.InfoText)
            .NotEmpty().WithMessage("Текст информации обязателен")
            .MaximumLength(2000).WithMessage("Текст не может превышать 2000 символов")
            .When(x => x.AdditionalInfo != null);
    }
}

public class UpdateAdditionalInfoCommandHandler(IAdditionalInfoRepository repository) : IRequestHandler<UpdateAdditionalInfoCommand, UpdateAdditionalInfoResult>
{
    private readonly IAdditionalInfoRepository _repository = repository;

    public async Task<UpdateAdditionalInfoResult> Handle(UpdateAdditionalInfoCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var existing = await _repository.GetAdditionalInfoByIdAsync(request.AdditionalInfo.InfoId, cancellationToken);
            if (existing == null)
                return UpdateAdditionalInfoResult.InfoNotFound();

            var updated = await _repository.UpdateAdditionalInfoAsync(request.AdditionalInfo, cancellationToken);

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
