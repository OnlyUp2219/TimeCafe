namespace TimeCafe.Application.CQRS.Clients.Command;

public record UpdateClientCommand(Client Client) : IRequest<Client>;

public class UpdateClientHandler : IRequestHandler<UpdateClientCommand, Client>
{
    private readonly IClientRepository _repository;

    public UpdateClientHandler(IClientRepository repository)
    {
        _repository = repository;
    }

    public async Task<Client> Handle(UpdateClientCommand request, CancellationToken cancellationToken)
    {
        return await _repository.UpdateClientAsync(request.Client);
    }
}
