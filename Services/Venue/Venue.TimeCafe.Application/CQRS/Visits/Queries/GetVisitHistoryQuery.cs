namespace Venue.TimeCafe.Application.CQRS.Visits.Queries;

public record GetVisitHistoryQuery(Guid UserId, int PageNumber, int PageSize) : IRequest<GetVisitHistoryResult>;

public record GetVisitHistoryResult(
    bool Success,
    string? Code = null,
    string? Message = null,
    int? StatusCode = null,
    List<ErrorItem>? Errors = null,
    IEnumerable<VisitWithTariffDto>? Visits = null) : ICqrsResult
{
    public static GetVisitHistoryResult GetFailed() =>
        new(false, Code: "GetVisitHistoryFailed", Message: "Не удалось получить историю посещений", StatusCode: 500);

    public static GetVisitHistoryResult GetSuccess(IEnumerable<VisitWithTariffDto> visits) =>
        new(true, Visits: visits);
}

public class GetVisitHistoryQueryValidator : AbstractValidator<GetVisitHistoryQuery>
{
    public GetVisitHistoryQueryValidator()
    {
        RuleFor(x => x.UserId).ValidGuidEntityId("Пользователь не найден");

        RuleFor(x => x.PageNumber).ValidPageNumber();

        RuleFor(x => x.PageSize).ValidPageSize();
    }
}

public class GetVisitHistoryQueryHandler(IVisitRepository repository) : IRequestHandler<GetVisitHistoryQuery, GetVisitHistoryResult>
{
    private readonly IVisitRepository _repository = repository;

    public async Task<GetVisitHistoryResult> Handle(GetVisitHistoryQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var visits = await _repository.GetVisitHistoryByUserAsync(request.UserId, request.PageNumber, request.PageSize);
            return GetVisitHistoryResult.GetSuccess(visits);
        }
        catch (Exception ex)
        {
            throw new CqrsResultException(GetVisitHistoryResult.GetFailed(), ex);
        }
    }
}
