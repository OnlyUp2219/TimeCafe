namespace Main.TimeCafe.Application.CQRS.Financials.Get;

public record GetFinancialTransactionByIdQuery(int TransactionId) : IRequest<FinancialTransaction?>;

public class GetFinancialTransactionByIdHandler(IFinancialRepository repository) : IRequestHandler<GetFinancialTransactionByIdQuery, FinancialTransaction?>
{
    private readonly IFinancialRepository _repository = repository;

    public async Task<FinancialTransaction?> Handle(GetFinancialTransactionByIdQuery request, CancellationToken cancellationToken)
    {
        var transactions = await _repository.GetClientTransactionsAsync(request.TransactionId);
        return transactions?.FirstOrDefault();
    }
}
