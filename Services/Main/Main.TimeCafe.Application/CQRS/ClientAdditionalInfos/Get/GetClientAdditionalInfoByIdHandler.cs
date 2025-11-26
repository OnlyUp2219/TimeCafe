namespace Main.TimeCafe.Application.CQRS.ClientAdditionalInfos.Get;

public record class GetClientAdditionalInfoByIdQuery(int infoId) : IRequest<ClientAdditionalInfo>;

public class GetClientAdditionalInfoByIdHandler(
    IClientAdditionalInfoRepository repository) : IRequestHandler<GetClientAdditionalInfoByIdQuery, ClientAdditionalInfo>
{
    private readonly IClientAdditionalInfoRepository _repository = repository;

    public async Task<ClientAdditionalInfo> Handle(GetClientAdditionalInfoByIdQuery request, CancellationToken cancellationToken)
    {
        return await _repository.GetAdditionalInfoByIdAsync(request.infoId);
    }
}



