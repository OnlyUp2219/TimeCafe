namespace Main.TimeCafe.Application.CQRS.Visits.Get;

public record IsClientAlreadyEnteredQuery(int ClientId) : IRequest<bool>;

public class IsClientAlreadyEnteredHandler(IVisitRepository repository) : IRequestHandler<IsClientAlreadyEnteredQuery, bool>
{
    private readonly IVisitRepository _repository = repository;

    public async Task<bool> Handle(IsClientAlreadyEnteredQuery request, CancellationToken cancellationToken)
    {
        return await _repository.IsClientAlreadyEnteredAsync(request.ClientId);
    }
}
