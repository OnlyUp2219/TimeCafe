namespace Main.TimeCafe.Application.CQRS.Clients.Get;

public record GetClientByIdQuery(int ClientId) : IRequest<Client?>;

public class GetClientByIdHandler(IClientRepository repository) : IRequestHandler<GetClientByIdQuery, Client?>
{
    private readonly IClientRepository _repository = repository;

    public async Task<Client?> Handle(GetClientByIdQuery request, CancellationToken cancellationToken)
    {
        return await _repository.GetClientByIdAsync(request.ClientId);
    }
}
