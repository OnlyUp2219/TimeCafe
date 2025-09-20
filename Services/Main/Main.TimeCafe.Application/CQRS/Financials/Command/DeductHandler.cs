namespace Main.TimeCafe.Application.CQRS.Financials.Command;

public record DeductCommand(int ClientId, decimal Amount, int? VisitId = null, string? Comment = null) : IRequest<FinancialTransaction>;

public class DeductHandler : IRequestHandler<DeductCommand, FinancialTransaction>
{
    private readonly IFinancialRepository _repository;

    public DeductHandler(IFinancialRepository repository)
    {
        _repository = repository;
    }

    public async Task<FinancialTransaction> Handle(DeductCommand request, CancellationToken cancellationToken)
    {
        return await _repository.DeductAsync(request.ClientId, request.Amount, request.VisitId, request.Comment);
    }
}
