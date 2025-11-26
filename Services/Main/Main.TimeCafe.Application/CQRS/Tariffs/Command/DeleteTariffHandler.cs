namespace Main.TimeCafe.Application.CQRS.Tariffs.Command;

public record class DeleteTariffСommand(int tariffId) : IRequest<bool>;
public class DeleteTariffHandler(ITariffRepository repository) : IRequestHandler<DeleteTariffСommand, bool>
{
    private readonly ITariffRepository _repository = repository;

    public async Task<bool> Handle(DeleteTariffСommand request, CancellationToken cancellationToken)
    {
        return await _repository.DeleteTariffAsync(request.tariffId);
    }
}