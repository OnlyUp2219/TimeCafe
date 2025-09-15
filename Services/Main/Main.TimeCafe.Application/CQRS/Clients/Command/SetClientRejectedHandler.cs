namespace TimeCafe.Application.CQRS.Clients.Command;

public record SetClientRejectedCommand(int ClientId, string Reason) : IRequest<bool>;

public class SetClientRejectedHandler : IRequestHandler<SetClientRejectedCommand, bool>
{
    private readonly IClientRepository _repository;

    public SetClientRejectedHandler(IClientRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(SetClientRejectedCommand request, CancellationToken cancellationToken)
    {
        return await _repository.SetClientRejectedAsync(request.ClientId, request.Reason);
    }
}
