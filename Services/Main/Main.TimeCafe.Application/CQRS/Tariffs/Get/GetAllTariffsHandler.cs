namespace TimeCafe.Application.CQRS.Tariffs.Get;

public record class GetAllTariffsQuery() : IRequest<IEnumerable<Tariff>>;

public class GetAllTariffsHandler : IRequestHandler<GetAllTariffsQuery, IEnumerable<Tariff>>
{
    private readonly ITariffRepository _repository;
    public GetAllTariffsHandler(ITariffRepository repository)
    {
        _repository = repository;
    }
    public async Task<IEnumerable<Tariff>> Handle(GetAllTariffsQuery request, CancellationToken cancellationToken)
    {
        return await _repository.GetAllTariffsAsync();
    }
}
