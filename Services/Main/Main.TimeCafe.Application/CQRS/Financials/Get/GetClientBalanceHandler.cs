namespace TimeCafe.Application.CQRS.Financials.Get;

public record GetClientBalanceQuery(int ClientId) : IRequest<decimal>;

public class GetClientBalanceHandler : IRequestHandler<GetClientBalanceQuery, decimal>
{
    private readonly IFinancialRepository _repository;

    public GetClientBalanceHandler(IFinancialRepository repository)
    {
        _repository = repository;
    }

    public async Task<decimal> Handle(GetClientBalanceQuery request, CancellationToken cancellationToken)
    {
        return await _repository.GetClientBalanceAsync(request.ClientId);
    }
}
