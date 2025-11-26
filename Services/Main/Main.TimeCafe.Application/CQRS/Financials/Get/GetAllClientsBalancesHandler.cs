namespace Main.TimeCafe.Application.CQRS.Financials.Get;

public record GetAllClientsBalancesQuery() : IRequest<IEnumerable<ClientBalanceDto>>;

public class GetAllClientsBalancesHandler(IFinancialRepository repository) : IRequestHandler<GetAllClientsBalancesQuery, IEnumerable<ClientBalanceDto>>
{
    private readonly IFinancialRepository _repository = repository;

    public async Task<IEnumerable<ClientBalanceDto>> Handle(GetAllClientsBalancesQuery request, CancellationToken cancellationToken)
    {
        return await _repository.GetAllClientsBalancesAsync();
    }
}
