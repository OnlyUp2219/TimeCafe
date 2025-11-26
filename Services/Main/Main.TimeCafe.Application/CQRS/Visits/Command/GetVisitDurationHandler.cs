namespace Main.TimeCafe.Application.CQRS.Visits.Command;

public record GetVisitDurationCommand(Visit Visit) : IRequest<TimeSpan>;

public class GetVisitDurationHandler(IVisitRepository repository) : IRequestHandler<GetVisitDurationCommand, TimeSpan>
{
    private readonly IVisitRepository _repository = repository;

    public Task<TimeSpan> Handle(GetVisitDurationCommand request, CancellationToken cancellationToken)
    {
        return Task.FromResult(_repository.GetVisitDuration(request.Visit));
    }
}
