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

public class UpdateAdditionalInfoCommandHandler(IUnitOfWork uow, IPublisher publisher) : ICommandHandler<UpdateAdditionalInfoCommand, AdditionalInfo>
{
    public async Task<Result<AdditionalInfo>> Handle(UpdateAdditionalInfoCommand request, CancellationToken cancellationToken = default)
    {
        try
        {
            var existing = await uow.AdditionalInfos.GetByIdAsync(request.InfoId, cancellationToken);
            if (existing == null)
                return Result.Fail(new InfoNotFoundError());

            var additionalInfo = new AdditionalInfo
            {
                InfoId = request.InfoId,
                UserId = request.UserId,
                InfoText = request.InfoText,
                CreatedAt = existing.CreatedAt,
                CreatedBy = request.CreatedBy,
            };

            //TODO : Mapping and updating CreatedBy
            var updated = await uow.AdditionalInfos.UpdateAsync(additionalInfo, cancellationToken);

            if (updated == null)
                return Result.Fail(new UpdateFailedError());

            await uow.SaveChangesAsync(cancellationToken);
            await publisher.Publish(new AdditionalInfoChangedEvent(request.UserId), cancellationToken);

            return Result.Ok(updated);
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error("Внутренняя ошибка").CausedBy(ex));
        }
    }
}
