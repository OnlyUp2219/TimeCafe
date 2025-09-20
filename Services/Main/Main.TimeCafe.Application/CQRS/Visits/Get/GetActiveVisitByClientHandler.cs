namespace Main.TimeCafe.Application.CQRS.Visits.Get;

public record GetActiveVisitByClientQuery(int ClientId) : IRequest<Visit>;

public class GetActiveVisitByClientHandler : IRequestHandler<GetActiveVisitByClientQuery, Visit>
{
    private readonly IVisitRepository _repository;

    public GetActiveVisitByClientHandler(IVisitRepository repository)
    {
        _repository = repository;
    }

    public async Task<Visit> Handle(GetActiveVisitByClientQuery request, CancellationToken cancellationToken)
    {
        return await _repository.GetActiveVisitByClientAsync(request.ClientId);
    }
}
