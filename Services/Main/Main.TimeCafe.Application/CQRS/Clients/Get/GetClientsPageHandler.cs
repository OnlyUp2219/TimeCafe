namespace Main.TimeCafe.Application.CQRS.Clients.Get;

public record GetClientsPageQuery(int PageNumber, int PageSize) : IRequest<IEnumerable<Client>>;

public class GetClientsPageHandler : IRequestHandler<GetClientsPageQuery, IEnumerable<Client>>
{
    private readonly IClientRepository _repository;

    public GetClientsPageHandler(IClientRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<Client>> Handle(GetClientsPageQuery request, CancellationToken cancellationToken)
    {
        return await _repository.GetClientsPageAsync(request.PageNumber, request.PageSize);
    }
}
