namespace Main.TimeCafe.Application.CQRS.Tariffs.Command;

public record class UpdateTariffCommand(Tariff tariff) : IRequest<Tariff>;
public class UpdateTariffHandler(ITariffRepository repository) : IRequestHandler<UpdateTariffCommand, Tariff>
{
    private readonly ITariffRepository _repository = repository;

    public async Task<Tariff> Handle(UpdateTariffCommand request, CancellationToken cancellationToken)
    {
        return await _repository.UpdateTariffAsync(request.tariff);
    }
}
