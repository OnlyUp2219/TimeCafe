namespace Main.TimeCafe.Application.CQRS.Clients.Command;

public record SetClientRejectedCommand(int ClientId, string Reason) : IRequest<bool>;

public class SetClientRejectedHandler(IClientRepository repository) : IRequestHandler<SetClientRejectedCommand, bool>
{
    private readonly IClientRepository _repository = repository;

    public async Task<bool> Handle(SetClientRejectedCommand request, CancellationToken cancellationToken)
    {
        return await _repository.SetClientRejectedAsync(request.ClientId, request.Reason);
    }
}
