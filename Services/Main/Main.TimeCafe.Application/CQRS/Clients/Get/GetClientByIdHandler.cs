namespace TimeCafe.Application.CQRS.Clients.Get;

public record GetClientByIdQuery(int ClientId) : IRequest<Client?>;

public class GetClientByIdHandler : IRequestHandler<GetClientByIdQuery, Client?>
{
    private readonly IClientRepository _repository;

    public GetClientByIdHandler(IClientRepository repository)
    {
        _repository = repository;
    }

    public async Task<Client?> Handle(GetClientByIdQuery request, CancellationToken cancellationToken)
    {
        return await _repository.GetClientByIdAsync(request.ClientId);
    }
}
