namespace Main.TimeCafe.Application.CQRS.Tariffs.Get;


public record class GetTariffByIdQuery(int tariffId) : IRequest<Tariff>;
public class GetTariffByIdHandler(ITariffRepository repository) : IRequestHandler<GetTariffByIdQuery, Tariff>
{
    private readonly ITariffRepository _repository = repository;

    public async Task<Tariff> Handle(GetTariffByIdQuery request, CancellationToken cancellationToken)
    {
        return await _repository.GetTariffByIdAsync(request.tariffId);
    }
}
