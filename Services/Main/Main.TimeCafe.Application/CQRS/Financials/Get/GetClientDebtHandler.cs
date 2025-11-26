namespace Main.TimeCafe.Application.CQRS.Financials.Get;

public record GetClientDebtQuery(int ClientId) : IRequest<decimal>;

public class GetClientDebtHandler(IFinancialRepository repository) : IRequestHandler<GetClientDebtQuery, decimal>
{
    private readonly IFinancialRepository _repository = repository;

    public async Task<decimal> Handle(GetClientDebtQuery request, CancellationToken cancellationToken)
    {
        return await _repository.GetClientDebtAsync(request.ClientId);
    }
}
