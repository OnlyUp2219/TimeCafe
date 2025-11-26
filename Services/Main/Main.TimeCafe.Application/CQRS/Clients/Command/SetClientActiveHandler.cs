namespace Main.TimeCafe.Application.CQRS.Clients.Command;

public record SetClientActiveCommand(int ClientId) : IRequest<bool>;

public class SetClientActiveHandler(IClientRepository repository) : IRequestHandler<SetClientActiveCommand, bool>
{
    private readonly IClientRepository _repository = repository;

    public async Task<bool> Handle(SetClientActiveCommand request, CancellationToken cancellationToken)
    {
        return await _repository.SetClientActiveAsync(request.ClientId);
    }
}
