namespace TimeCafe.Application.CQRS.Financials.Get;

public record GetClientsWithDebtQuery() : IRequest<IEnumerable<ClientBalanceDto>>;

public class GetClientsWithDebtHandler : IRequestHandler<GetClientsWithDebtQuery, IEnumerable<ClientBalanceDto>>
{
    private readonly IFinancialRepository _repository;

    public GetClientsWithDebtHandler(IFinancialRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<ClientBalanceDto>> Handle(GetClientsWithDebtQuery request, CancellationToken cancellationToken)
    {
        return await _repository.GetClientsWithDebtAsync();
    }
}
