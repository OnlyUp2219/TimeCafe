namespace Venue.TimeCafe.Application.CQRS.Visits.Queries;

public record GetVisitsPageQuery(int PageNumber, int PageSize) : IRequest<GetVisitsPageResult>;

public record GetVisitsPageResult(
    bool Success,
    string? Code = null,
    string? Message = null,
    int? StatusCode = null,
    List<ErrorItem>? Errors = null,
    IEnumerable<VisitWithTariffDto>? Visits = null,
    int TotalCount = 0) : ICqrsResult
{
    public static GetVisitsPageResult GetFailed() =>
        new(false, Code: "GetVisitsPageFailed", Message: "Не удалось получить страницу визитов", StatusCode: 500);

    public static GetVisitsPageResult GetSuccess(IEnumerable<VisitWithTariffDto> visits, int totalCount) =>
        new(true, Visits: visits, TotalCount: totalCount);
}

public class GetVisitsPageQueryValidator : AbstractValidator<GetVisitsPageQuery>
{
    public GetVisitsPageQueryValidator()
    {
        RuleFor(x => x.PageNumber).ValidPageNumber();
        RuleFor(x => x.PageSize).ValidPageSize();
    }
}

public class GetVisitsPageQueryHandler(IVisitRepository repository) : IRequestHandler<GetVisitsPageQuery, GetVisitsPageResult>
{
    private readonly IVisitRepository _repository = repository;

    public async Task<GetVisitsPageResult> Handle(GetVisitsPageQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var visits = await _repository.GetPagedAsync(request.PageNumber, request.PageSize);
            var totalCount = await _repository.GetTotalCountAsync();

            return GetVisitsPageResult.GetSuccess(visits, totalCount);
        }
        catch (Exception ex)
        {
            throw new CqrsResultException(GetVisitsPageResult.GetFailed(), ex);
        }
    }
}
