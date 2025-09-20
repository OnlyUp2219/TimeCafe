namespace Main.TimeCafe.Application.CQRS.Tariffs.Get;

public record class GetTariffsPageQuery(int pageNumber, int pageSize) : IRequest<IEnumerable<Tariff>>;
public class GetTariffsPageHandler : IRequestHandler<GetTariffsPageQuery, IEnumerable<Tariff>>
{
    private readonly ITariffRepository _repository;
    public GetTariffsPageHandler(ITariffRepository repository)
    {
        _repository = repository;
    }
    public async Task<IEnumerable<Tariff>> Handle(GetTariffsPageQuery request, CancellationToken cancellationToken)
    {
        return await _repository.GetTariffsPageAsync(request.pageNumber, request.pageSize);
    }
}
