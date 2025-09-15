namespace TimeCafe.Application.CQRS.Tariffs.Command;

public record class UpdateTariffCommand(Tariff tariff) : IRequest<Tariff>;
public class UpdateTariffHandler : IRequestHandler<UpdateTariffCommand, Tariff>
{
    private readonly ITariffRepository _repository;
    public UpdateTariffHandler(ITariffRepository repository)
    {
        _repository = repository;
    }
    public async Task<Tariff> Handle(UpdateTariffCommand request, CancellationToken cancellationToken)
    {
        return await _repository.UpdateTariffAsync(request.tariff);
    }
}
