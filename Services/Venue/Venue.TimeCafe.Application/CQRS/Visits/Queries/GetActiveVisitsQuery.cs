using Venue.TimeCafe.Application.Contracts.Repositories;

namespace Venue.TimeCafe.Application.CQRS.Visits.Queries;

public record GetActiveVisitsQuery() : IRequest<GetActiveVisitsResult>;

public record GetActiveVisitsResult(
    bool Success,
    string? Code = null,
    string? Message = null,
    int? StatusCode = null,
    List<ErrorItem>? Errors = null,
    IEnumerable<VisitWithTariffDto>? Visits = null) : ICqrsResultV2
{
    public static GetActiveVisitsResult GetFailed() =>
        new(false, Code: "GetActiveVisitsFailed", Message: "Не удалось получить активные посещения", StatusCode: 500);

    public static GetActiveVisitsResult GetSuccess(IEnumerable<VisitWithTariffDto> visits) =>
        new(true, Visits: visits);
}

public class GetActiveVisitsQueryHandler(IVisitRepository repository) : IRequestHandler<GetActiveVisitsQuery, GetActiveVisitsResult>
{
    private readonly IVisitRepository _repository = repository;

    public async Task<GetActiveVisitsResult> Handle(GetActiveVisitsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var visits = await _repository.GetActiveVisitsAsync();
            return GetActiveVisitsResult.GetSuccess(visits);
        }
        catch (Exception)
        {
            return GetActiveVisitsResult.GetFailed();
        }
    }
}
