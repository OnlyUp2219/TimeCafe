namespace Main.TimeCafe.Application.CQRS.ClientAdditionalInfos.Command;

public record class UpdateAdditionalInfoCommand(ClientAdditionalInfo info) : IRequest<ClientAdditionalInfo>;
public class UpdateAdditionalInfoHandler(
    IClientAdditionalInfoRepository repository) : IRequestHandler<UpdateAdditionalInfoCommand, ClientAdditionalInfo>
{
    private readonly IClientAdditionalInfoRepository _repository = repository;

    public async Task<ClientAdditionalInfo> Handle(UpdateAdditionalInfoCommand request, CancellationToken cancellationToken)
    {
        return await _repository.UpdateAdditionalInfoAsync(request.info);
    }
}
