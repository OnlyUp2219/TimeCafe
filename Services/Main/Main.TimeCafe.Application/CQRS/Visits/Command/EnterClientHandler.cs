namespace TimeCafe.Application.CQRS.Visits.Command;

public record EnterClientCommand(int ClientId, int TariffId, int MinimumEntryMinutes) : IRequest<Visit>;

public class EnterClientHandler : IRequestHandler<EnterClientCommand, Visit>
{
    private readonly IVisitRepository _repository;

    public EnterClientHandler(IVisitRepository repository)
    {
        _repository = repository;
    }

    public async Task<Visit> Handle(EnterClientCommand request, CancellationToken cancellationToken)
    {
        return await _repository.EnterClientAsync(request.ClientId, request.TariffId, request.MinimumEntryMinutes);
    }
}
