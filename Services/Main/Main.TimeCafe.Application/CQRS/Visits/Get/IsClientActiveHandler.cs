namespace TimeCafe.Application.CQRS.Visits.Get;

public record IsClientActiveQuery(int ClientId) : IRequest<bool>;

public class IsClientActiveHandler : IRequestHandler<IsClientActiveQuery, bool>
{
    private readonly IVisitRepository _repository;

    public IsClientActiveHandler(IVisitRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(IsClientActiveQuery request, CancellationToken cancellationToken)
    {
        return await _repository.IsClientActiveAsync(request.ClientId);
    }
}
