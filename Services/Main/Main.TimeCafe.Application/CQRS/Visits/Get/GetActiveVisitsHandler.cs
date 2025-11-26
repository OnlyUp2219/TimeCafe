namespace Main.TimeCafe.Application.CQRS.Visits.Get;

public record GetActiveVisitsQuery() : IRequest<IEnumerable<Visit>>;

public class GetActiveVisitsHandler(IVisitRepository repository) : IRequestHandler<GetActiveVisitsQuery, IEnumerable<Visit>>
{
    private readonly IVisitRepository _repository = repository;

    public async Task<IEnumerable<Visit>> Handle(GetActiveVisitsQuery request, CancellationToken cancellationToken)
    {
        return await _repository.GetActiveVisitsAsync();
    }
}
