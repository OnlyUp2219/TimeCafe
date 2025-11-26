namespace Main.TimeCafe.Application.CQRS.Financials.Get;

public record GetClientFinancialTransactionsQuery(int ClientId, int? Limit = null) : IRequest<IEnumerable<FinancialTransaction>>;

public class GetClientFinancialTransactionsHandler(IFinancialRepository repository) : IRequestHandler<GetClientFinancialTransactionsQuery, IEnumerable<FinancialTransaction>>
{
    private readonly IFinancialRepository _repository = repository;

    public async Task<IEnumerable<FinancialTransaction>> Handle(GetClientFinancialTransactionsQuery request, CancellationToken cancellationToken)
    {
        return await _repository.GetClientTransactionsAsync(request.ClientId, request.Limit);
    }
}
