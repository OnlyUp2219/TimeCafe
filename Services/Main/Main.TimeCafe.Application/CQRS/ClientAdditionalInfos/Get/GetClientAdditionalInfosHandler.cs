namespace Main.TimeCafe.Application.CQRS.ClientAdditionalInfos.Get;
public record class GetClientAdditionalInfosQuery(int clientId) : IRequest<IEnumerable<ClientAdditionalInfo>>;
public record class GetClientAdditionalInfosHandler : IRequestHandler<GetClientAdditionalInfosQuery, IEnumerable<ClientAdditionalInfo>>
{
    private readonly IClientAdditionalInfoRepository _repository;
    public GetClientAdditionalInfosHandler(
        IClientAdditionalInfoRepository repository)
    {
        _repository = repository;
    }
    public async Task<IEnumerable<ClientAdditionalInfo>> Handle(GetClientAdditionalInfosQuery request, CancellationToken cancellationToken)
    {
        return await _repository.GetClientAdditionalInfosAsync(request.clientId);
    }
}
