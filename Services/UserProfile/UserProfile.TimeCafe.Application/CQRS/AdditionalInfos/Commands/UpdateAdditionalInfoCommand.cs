namespace UserProfile.TimeCafe.Application.CQRS.AdditionalInfos.Commands;

public record UpdateAdditionalInfoCommand(Guid InfoId, Guid UserId, string InfoText, string? CreatedBy = null) : ICommand<AdditionalInfo>;

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

public class UpdateAdditionalInfoCommandHandler(IAdditionalInfoRepository repository) : ICommandHandler<UpdateAdditionalInfoCommand, AdditionalInfo>
{
    private readonly IAdditionalInfoRepository _repository = repository;

    public async Task<Result<AdditionalInfo>> Handle(UpdateAdditionalInfoCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var existing = await _repository.GetAdditionalInfoByIdAsync(request.InfoId, cancellationToken);
            if (existing == null)
                return Result.Fail(new InfoNotFoundError());

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
                return Result.Fail(new UpdateFailedError());

            return Result.Ok(updated);
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error("Внутренняя ошибка").CausedBy(ex));
        }
    }
}
