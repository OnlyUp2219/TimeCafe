namespace Main.TimeCafe.Application.CQRS.Clients.Get;

public record GetClientStatusesQuery() : IRequest<IEnumerable<ClientStatus>>;

public class GetClientStatusesHandler(IClientRepository repository) : IRequestHandler<GetClientStatusesQuery, IEnumerable<ClientStatus>>
{
    private readonly IClientRepository _repository = repository;

    public async Task<IEnumerable<ClientStatus>> Handle(GetClientStatusesQuery request, CancellationToken cancellationToken)
    {
        return await _repository.GetClientStatusesAsync();
    }
}
