namespace Main.TimeCafe.Application.CQRS.Clients.Get;

public record GetGendersQuery() : IRequest<IEnumerable<Gender>>;

public class GetGendersHandler : IRequestHandler<GetGendersQuery, IEnumerable<Gender>>
{
    private readonly IClientRepository _repository;

    public GetGendersHandler(IClientRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<Gender>> Handle(GetGendersQuery request, CancellationToken cancellationToken)
    {
        return await _repository.GetGendersAsync();
    }
}
