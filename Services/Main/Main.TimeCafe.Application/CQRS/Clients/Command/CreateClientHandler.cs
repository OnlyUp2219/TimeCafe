namespace TimeCafe.Application.CQRS.Clients.Command;

public record CreateClientCommand(Client Client) : IRequest<Client>;

public class CreateClientHandler : IRequestHandler<CreateClientCommand, Client>
{
    private readonly IClientRepository _repository;

    public CreateClientHandler(IClientRepository repository)
    {
        _repository = repository;
    }

    public async Task<Client> Handle(CreateClientCommand request, CancellationToken cancellationToken)
    {
        return await _repository.CreateClientAsync(request.Client);
    }
}
