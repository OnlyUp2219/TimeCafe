namespace Main.TimeCafe.Application.CQRS.Clients.Command;

public record UpdateClientStatusCommand(int ClientId, int StatusId) : IRequest<bool>;

public class UpdateClientStatusHandler : IRequestHandler<UpdateClientStatusCommand, bool>
{
    private readonly IClientRepository _repository;

    public UpdateClientStatusHandler(IClientRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(UpdateClientStatusCommand request, CancellationToken cancellationToken)
    {
        return await _repository.UpdateClientStatusAsync(request.ClientId, request.StatusId);
    }
}
