namespace TimeCafe.Application.CQRS.Visits.Command;

public record ExitClientCommand(int VisitId) : IRequest<Visit>;

public class ExitClientHandler : IRequestHandler<ExitClientCommand, Visit>
{
    private readonly IVisitRepository _repository;

    public ExitClientHandler(IVisitRepository repository)
    {
        _repository = repository;
    }

    public async Task<Visit> Handle(ExitClientCommand request, CancellationToken cancellationToken)
    {
        return await _repository.ExitClientAsync(request.VisitId);
    }
}
