namespace UserProfile.TimeCafe.Application.CQRS.AdditionalInfos.Commands;

public record DeleteAdditionalInfoCommand(Guid InfoId) : ICommand;

public class DeleteAdditionalInfoCommandValidator : AbstractValidator<DeleteAdditionalInfoCommand>
{
    public DeleteAdditionalInfoCommandValidator()
    {
        RuleFor(x => x.InfoId).ValidGuidEntityId("Информации отсутствует");
    }
}

public class DeleteAdditionalInfoCommandHandler(IAdditionalInfoRepository repository) : ICommandHandler<DeleteAdditionalInfoCommand>
{
    private readonly IAdditionalInfoRepository _repository = repository;

    public async Task<Result> Handle(DeleteAdditionalInfoCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var deleted = await _repository.DeleteAdditionalInfoAsync(request.InfoId, cancellationToken);

            if (!deleted)
                return Result.Fail(new InfoNotFoundError());

            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error("Внутренняя ошибка").CausedBy(ex));
        }
    }
}
