namespace Main.TimeCafe.Application.CQRS.ClientAdditionalInfos.Command;

public record class DeleteAdditionalInfoCommand(int infoId) : IRequest<bool>;
public class DeleteAdditionalInfoHandler(
    IClientAdditionalInfoRepository repository) : IRequestHandler<DeleteAdditionalInfoCommand, bool>
{
    private readonly IClientAdditionalInfoRepository _repository = repository;

    public async Task<bool> Handle(DeleteAdditionalInfoCommand request, CancellationToken cancellationToken)
    {
        return await _repository.DeleteAdditionalInfoAsync(request.infoId);
    }
}
