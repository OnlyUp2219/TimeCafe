namespace TimeCafe.Application.CQRS.ClientAdditionalInfos.Command;
public record class UpdateAdditionalInfoCommand(ClientAdditionalInfo info) : IRequest<ClientAdditionalInfo>;
public class UpdateAdditionalInfoHandler : IRequestHandler<UpdateAdditionalInfoCommand, ClientAdditionalInfo>
{
    private readonly IClientAdditionalInfoRepository _repository;
    public UpdateAdditionalInfoHandler(
        IClientAdditionalInfoRepository repository)
    {
        _repository = repository;
    }
    public async Task<ClientAdditionalInfo> Handle(UpdateAdditionalInfoCommand request, CancellationToken cancellationToken)
    {
        return await _repository.UpdateAdditionalInfoAsync(request.info);
    }
}
