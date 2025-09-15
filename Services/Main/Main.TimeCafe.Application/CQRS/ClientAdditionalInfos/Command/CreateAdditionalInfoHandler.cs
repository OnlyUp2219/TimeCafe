namespace TimeCafe.Application.CQRS.ClientAdditionalInfos.Command;
public record class CreateAdditionalInfoCommand(ClientAdditionalInfo info) : IRequest<ClientAdditionalInfo>;
public class CreateAdditionalInfoHandler : IRequestHandler<CreateAdditionalInfoCommand, ClientAdditionalInfo>
{
    private readonly IClientAdditionalInfoRepository _repository;
    public CreateAdditionalInfoHandler(
        IClientAdditionalInfoRepository repository)
    {
        _repository = repository;
    }
    public async Task<ClientAdditionalInfo> Handle(CreateAdditionalInfoCommand request, CancellationToken cancellationToken)
    {
        return await _repository.CreateAdditionalInfoAsync(request.info);
    }
}
