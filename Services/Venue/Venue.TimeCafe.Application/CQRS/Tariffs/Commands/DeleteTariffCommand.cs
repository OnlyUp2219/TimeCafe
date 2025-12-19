using Venue.TimeCafe.Application.Contracts.Repositories;

namespace Venue.TimeCafe.Application.CQRS.Tariffs.Commands;

public record DeleteTariffCommand(string TariffId) : IRequest<DeleteTariffResult>;

public record DeleteTariffResult(
    bool Success,
    string? Code = null,
    string? Message = null,
    int? StatusCode = null,
    List<ErrorItem>? Errors = null) : ICqrsResultV2
{
    public static DeleteTariffResult TariffNotFound() =>
        new(false, Code: "TariffNotFound", Message: "Тариф не найден", StatusCode: 404);

    public static DeleteTariffResult DeleteFailed() =>
        new(false, Code: "DeleteTariffFailed", Message: "Не удалось удалить тариф", StatusCode: 500);

    public static DeleteTariffResult DeleteSuccess() =>
        new(true, Message: "Тариф успешно удалён");
}

public class DeleteTariffCommandValidator : AbstractValidator<DeleteTariffCommand>
{
    public DeleteTariffCommandValidator()
    {
        RuleFor(x => x.TariffId)
                   .NotEmpty().WithMessage("Тариф не найден")
                   .Must(x => !string.IsNullOrWhiteSpace(x)).WithMessage("Тариф не найден")
                   .Must(x => Guid.TryParse(x, out var guid) && guid != Guid.Empty).WithMessage("Тариф не найден");
    }
}

public class DeleteTariffCommandHandler(ITariffRepository repository) : IRequestHandler<DeleteTariffCommand, DeleteTariffResult>
{
    private readonly ITariffRepository _repository = repository;

    public async Task<DeleteTariffResult> Handle(DeleteTariffCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var tariffId = Guid.Parse(request.TariffId);

            var existing = await _repository.GetByIdAsync(tariffId);
            if (existing == null)
                return DeleteTariffResult.TariffNotFound();

            var result = await _repository.DeleteAsync(tariffId);

            if (!result)
                return DeleteTariffResult.DeleteFailed();

            return DeleteTariffResult.DeleteSuccess();
        }
        catch (Exception)
        {
            return DeleteTariffResult.DeleteFailed();
        }
    }
}
