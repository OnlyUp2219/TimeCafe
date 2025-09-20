namespace Main.TimeCafe.Application.CQRS.Financials.Get;

public record GetAllClientsBalancesQuery() : IRequest<IEnumerable<ClientBalanceDto>>;

public class GetAllClientsBalancesHandler : IRequestHandler<GetAllClientsBalancesQuery, IEnumerable<ClientBalanceDto>>
{
    private readonly IFinancialRepository _repository;

    public GetAllClientsBalancesHandler(IFinancialRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<ClientBalanceDto>> Handle(GetAllClientsBalancesQuery request, CancellationToken cancellationToken)
    {
        return await _repository.GetAllClientsBalancesAsync();
    }
}
