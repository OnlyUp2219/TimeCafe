namespace TimeCafe.Application.CQRS.Tariffs.Get;
public record class GetTotalPageTariffQuery() : IRequest<int>;
public class GetTotalPageTariffHandler : IRequestHandler<GetTotalPageTariffQuery, int>
{
    private readonly ITariffRepository _repository;
    public GetTotalPageTariffHandler(ITariffRepository repository)
    {
        _repository = repository;
    }
    public async Task<int> Handle(GetTotalPageTariffQuery request, CancellationToken cancellationToken)
    {
        return await _repository.GetTotalPageAsync();
    }
}
