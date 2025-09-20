namespace Main.TimeCafe.Application.CQRS.Visits.Command;

public record CalculateVisitCostCommand(Visit Visit) : IRequest<decimal>;

public class CalculateVisitCostHandler : IRequestHandler<CalculateVisitCostCommand, decimal>
{
    private readonly IVisitRepository _repository;

    public CalculateVisitCostHandler(IVisitRepository repository)
    {
        _repository = repository;
    }

    public async Task<decimal> Handle(CalculateVisitCostCommand request, CancellationToken cancellationToken)
    {
        return await _repository.CalculateVisitCostAsync(request.Visit);
    }
}
