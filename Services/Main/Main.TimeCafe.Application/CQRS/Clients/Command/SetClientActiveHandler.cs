namespace TimeCafe.Application.CQRS.Clients.Command;

public record SetClientActiveCommand(int ClientId) : IRequest<bool>;

public class SetClientActiveHandler : IRequestHandler<SetClientActiveCommand, bool>
{
    private readonly IClientRepository _repository;

    public SetClientActiveHandler(IClientRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(SetClientActiveCommand request, CancellationToken cancellationToken)
    {
        return await _repository.SetClientActiveAsync(request.ClientId);
    }
}
