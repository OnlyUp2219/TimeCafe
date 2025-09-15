namespace TimeCafe.Application.CQRS.Clients.Get;

public record IsPhoneConfirmedQuery(int ClientId) : IRequest<bool>;

public class IsPhoneConfirmedHandler : IRequestHandler<IsPhoneConfirmedQuery, bool>
{
    private readonly IClientRepository _repository;

    public IsPhoneConfirmedHandler(IClientRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(IsPhoneConfirmedQuery request, CancellationToken cancellationToken)
    {
        return await _repository.IsPhoneConfirmedAsync(request.ClientId);
    }
}
