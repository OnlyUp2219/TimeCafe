namespace Main.TimeCafe.Application.CQRS.Clients.Get;

public record GetAllClientsQuery() : IRequest<IEnumerable<Client>>;

public class GetAllClientsHandler : IRequestHandler<GetAllClientsQuery, IEnumerable<Client>>
{
    private readonly IClientRepository _repository;

    public GetAllClientsHandler(IClientRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<Client>> Handle(GetAllClientsQuery request, CancellationToken cancellationToken)
    {
        return await _repository.GetAllClientsAsync();
    }
}
