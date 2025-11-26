namespace Main.TimeCafe.Application.CQRS.Clients.Command;

public record SetClientDraftCommand(int ClientId) : IRequest<bool>;

public class SetClientDraftHandler(IClientRepository repository) : IRequestHandler<SetClientDraftCommand, bool>
{
    private readonly IClientRepository _repository = repository;

    public async Task<bool> Handle(SetClientDraftCommand request, CancellationToken cancellationToken)
    {
        return await _repository.SetClientDraftAsync(request.ClientId);
    }
}
