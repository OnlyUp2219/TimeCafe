namespace TimeCafe.Application.CQRS.Clients.Get;

public record GetClientStatusesQuery() : IRequest<IEnumerable<ClientStatus>>;

public class GetClientStatusesHandler : IRequestHandler<GetClientStatusesQuery, IEnumerable<ClientStatus>>
{
    private readonly IClientRepository _repository;

    public GetClientStatusesHandler(IClientRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<ClientStatus>> Handle(GetClientStatusesQuery request, CancellationToken cancellationToken)
    {
        return await _repository.GetClientStatusesAsync();
    }
}
