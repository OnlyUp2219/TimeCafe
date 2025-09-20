namespace Main.TimeCafe.Application.CQRS.Financials.Command;

public record DepositCommand(int ClientId, decimal Amount, string? Comment = null) : IRequest<FinancialTransaction>;

public class DepositHandler : IRequestHandler<DepositCommand, FinancialTransaction>
{
    private readonly IFinancialRepository _repository;

    public DepositHandler(IFinancialRepository repository)
    {
        _repository = repository;
    }

    public async Task<FinancialTransaction> Handle(DepositCommand request, CancellationToken cancellationToken)
    {
        return await _repository.DepositAsync(request.ClientId, request.Amount, request.Comment);
    }
}
