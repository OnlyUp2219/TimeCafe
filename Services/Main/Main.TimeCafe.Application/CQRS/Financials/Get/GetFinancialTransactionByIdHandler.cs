namespace TimeCafe.Application.CQRS.Financials.Get;

public record GetFinancialTransactionByIdQuery(int TransactionId) : IRequest<FinancialTransaction?>;

public class GetFinancialTransactionByIdHandler : IRequestHandler<GetFinancialTransactionByIdQuery, FinancialTransaction?>
{
    private readonly IFinancialRepository _repository;

    public GetFinancialTransactionByIdHandler(IFinancialRepository repository)
    {
        _repository = repository;
    }

    public async Task<FinancialTransaction?> Handle(GetFinancialTransactionByIdQuery request, CancellationToken cancellationToken)
    {
        var transactions = await _repository.GetClientTransactionsAsync(request.TransactionId);
        return transactions?.FirstOrDefault();
    }
}
