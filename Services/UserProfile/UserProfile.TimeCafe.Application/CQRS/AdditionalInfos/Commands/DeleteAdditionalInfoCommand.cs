namespace UserProfile.TimeCafe.Application.CQRS.AdditionalInfos.Commands;

public record DeleteAdditionalInfoCommand(Guid InfoId) : ICommand;

public class DeleteAdditionalInfoCommandValidator : AbstractValidator<DeleteAdditionalInfoCommand>
{
    public DeleteAdditionalInfoCommandValidator()
    {
        RuleFor(x => x.InfoId).ValidGuidEntityId("Информации отсутствует");
    }
}

public class DeleteAdditionalInfoCommandHandler(IUnitOfWork uow, IPublisher publisher) : ICommandHandler<DeleteAdditionalInfoCommand>
{
    public async Task<Result> Handle(DeleteAdditionalInfoCommand request, CancellationToken cancellationToken = default)
    {
        try
        {
            var existing = await uow.AdditionalInfos.GetByIdAsync(request.InfoId, cancellationToken);
            if (existing == null)
                return Result.Fail(new InfoNotFoundError());

            await uow.AdditionalInfos.DeleteAsync(request.InfoId, cancellationToken);
            await uow.SaveChangesAsync(cancellationToken);
            await publisher.Publish(new AdditionalInfoChangedEvent(existing.UserId), cancellationToken);

            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error("Внутренняя ошибка").CausedBy(ex));
        }
    }
}
