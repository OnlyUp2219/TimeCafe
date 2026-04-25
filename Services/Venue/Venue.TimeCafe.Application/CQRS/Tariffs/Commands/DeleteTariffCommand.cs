namespace Venue.TimeCafe.Application.CQRS.Tariffs.Commands;

public record DeleteTariffCommand(Guid TariffId) : ICommand;

public class DeleteTariffCommandValidator : AbstractValidator<DeleteTariffCommand>
{
    public DeleteTariffCommandValidator()
    {
        RuleFor(x => x.TariffId).ValidGuidEntityId("Тариф не найден");
    }
}

public class DeleteTariffCommandHandler(ITariffRepository repository) : ICommandHandler<DeleteTariffCommand>
{
    private readonly ITariffRepository _repository = repository;

    public async Task<Result> Handle(DeleteTariffCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var existing = await _repository.GetByIdAsync(request.TariffId);
            if (existing == null)
                return Result.Fail(new TariffNotFoundError());

            var result = await _repository.DeleteAsync(request.TariffId);

            if (!result)
                return Result.Fail(new DeleteFailedError());

            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error("Внутренняя ошибка").CausedBy(ex));
        }
    }
}

