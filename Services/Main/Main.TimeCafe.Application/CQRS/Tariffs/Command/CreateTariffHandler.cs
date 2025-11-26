namespace Main.TimeCafe.Application.CQRS.Tariffs.Command;

public record class CreateTariffCommand(Tariff tariff) : IRequest<Tariff>;

public class CreateTariffHandler(ITariffRepository repository) : IRequestHandler<CreateTariffCommand, Tariff>
{
    private readonly ITariffRepository _repository = repository;

    public async Task<Tariff> Handle(CreateTariffCommand request, CancellationToken cancellationToken)
    {
        return await _repository.CreateTariffAsync(request.tariff);
    }
}
