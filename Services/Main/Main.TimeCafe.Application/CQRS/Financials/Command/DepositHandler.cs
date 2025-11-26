namespace Main.TimeCafe.Application.CQRS.Financials.Command;

public record DepositCommand(int ClientId, decimal Amount, string? Comment = null) : IRequest<FinancialTransaction>;

public class DepositHandler(IFinancialRepository repository) : IRequestHandler<DepositCommand, FinancialTransaction>
{
    private readonly IFinancialRepository _repository = repository;

    public async Task<FinancialTransaction> Handle(DepositCommand request, CancellationToken cancellationToken)
    {
        return await _repository.DepositAsync(request.ClientId, request.Amount, request.Comment);
    }
}
