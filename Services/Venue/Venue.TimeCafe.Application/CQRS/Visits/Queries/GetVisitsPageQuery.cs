namespace Venue.TimeCafe.Application.CQRS.Visits.Queries;

public record GetVisitsPageQuery(int PageNumber, int PageSize) : IQuery<GetVisitsPageResponse>;

public record GetVisitsPageResponse(IEnumerable<VisitWithTariffDto> Visits, int TotalCount);

public class GetVisitsPageQueryValidator : AbstractValidator<GetVisitsPageQuery>
{
    public GetVisitsPageQueryValidator()
    {
        RuleFor(x => x.PageNumber).ValidPageNumber();
        RuleFor(x => x.PageSize).ValidPageSize();
    }
}

public class GetVisitsPageQueryHandler(IVisitRepository repository) : IQueryHandler<GetVisitsPageQuery, GetVisitsPageResponse>
{
    private readonly IVisitRepository _repository = repository;

    public async Task<Result<GetVisitsPageResponse>> Handle(GetVisitsPageQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var visits = await _repository.GetPagedAsync(request.PageNumber, request.PageSize);
            var totalCount = await _repository.GetTotalCountAsync();

            return Result.Ok(new GetVisitsPageResponse(visits, totalCount));
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error("Внутренняя ошибка").CausedBy(ex));
        }
    }
}

