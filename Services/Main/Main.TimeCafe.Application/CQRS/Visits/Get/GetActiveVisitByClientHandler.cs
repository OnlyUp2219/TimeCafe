namespace Main.TimeCafe.Application.CQRS.Visits.Get;

public record GetActiveVisitByClientQuery(int ClientId) : IRequest<Visit>;

public class GetActiveVisitByClientHandler(IVisitRepository repository) : IRequestHandler<GetActiveVisitByClientQuery, Visit>
{
    private readonly IVisitRepository _repository = repository;

    public async Task<Visit> Handle(GetActiveVisitByClientQuery request, CancellationToken cancellationToken)
    {
        return await _repository.GetActiveVisitByClientAsync(request.ClientId);
    }
}
