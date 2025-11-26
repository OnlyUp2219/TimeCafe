namespace Main.TimeCafe.Application.CQRS.Clients.Get;

public record GetTotalPageClientQuery() : IRequest<int>;

public class GetTotalPageClientHandler(IClientRepository repository) : IRequestHandler<GetTotalPageClientQuery, int>
{
    private readonly IClientRepository _repository = repository;

    public async Task<int> Handle(GetTotalPageClientQuery request, CancellationToken cancellationToken)
    {
        return await _repository.GetTotalPageAsync();
    }
}
