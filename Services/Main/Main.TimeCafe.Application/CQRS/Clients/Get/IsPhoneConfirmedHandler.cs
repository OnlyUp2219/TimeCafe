namespace Main.TimeCafe.Application.CQRS.Clients.Get;

public record IsPhoneConfirmedQuery(int ClientId) : IRequest<bool>;

public class IsPhoneConfirmedHandler(IClientRepository repository) : IRequestHandler<IsPhoneConfirmedQuery, bool>
{
    private readonly IClientRepository _repository = repository;

    public async Task<bool> Handle(IsPhoneConfirmedQuery request, CancellationToken cancellationToken)
    {
        return await _repository.IsPhoneConfirmedAsync(request.ClientId);
    }
}
