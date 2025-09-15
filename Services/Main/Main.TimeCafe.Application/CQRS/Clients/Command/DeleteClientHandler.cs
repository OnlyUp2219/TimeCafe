namespace TimeCafe.Application.CQRS.Clients.Command;

public record DeleteClientCommand(int ClientId) : IRequest<bool>;

public class DeleteClientHandler : IRequestHandler<DeleteClientCommand, bool>
{
    private readonly IClientRepository _repository;

    public DeleteClientHandler(IClientRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(DeleteClientCommand request, CancellationToken cancellationToken)
    {
        return await _repository.DeleteClientAsync(request.ClientId);
    }
}
