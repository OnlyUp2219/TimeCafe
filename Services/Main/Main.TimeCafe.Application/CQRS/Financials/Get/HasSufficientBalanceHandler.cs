namespace TimeCafe.Application.CQRS.Financials.Get;

public record HasSufficientBalanceQuery(int ClientId, decimal RequiredAmount) : IRequest<bool>;

public class HasSufficientBalanceHandler : IRequestHandler<HasSufficientBalanceQuery, bool>
{
    private readonly IFinancialRepository _repository;

    public HasSufficientBalanceHandler(IFinancialRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(HasSufficientBalanceQuery request, CancellationToken cancellationToken)
    {
        return await _repository.HasSufficientBalanceAsync(request.ClientId, request.RequiredAmount);
    }
}
