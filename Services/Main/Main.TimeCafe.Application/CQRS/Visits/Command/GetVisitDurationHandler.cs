namespace TimeCafe.Application.CQRS.Visits.Command;

public record GetVisitDurationCommand(Visit Visit) : IRequest<TimeSpan>;

public class GetVisitDurationHandler : IRequestHandler<GetVisitDurationCommand, TimeSpan>
{
    private readonly IVisitRepository _repository;

    public GetVisitDurationHandler(IVisitRepository repository)
    {
        _repository = repository;
    }

    public Task<TimeSpan> Handle(GetVisitDurationCommand request, CancellationToken cancellationToken)
    {
        return Task.FromResult(_repository.GetVisitDuration(request.Visit));
    }
}
