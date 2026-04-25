namespace Venue.TimeCafe.Application.CQRS.Visits.Queries;

public record GetVisitHistoryQuery(Guid UserId, int PageNumber, int PageSize) : IQuery<IEnumerable<VisitWithTariffDto>>;

public class GetVisitHistoryQueryValidator : AbstractValidator<GetVisitHistoryQuery>
{
    public GetVisitHistoryQueryValidator()
    {
        RuleFor(x => x.UserId).ValidGuidEntityId("Пользователь не найден");

        RuleFor(x => x.PageNumber).ValidPageNumber();

        RuleFor(x => x.PageSize).ValidPageSize();
    }
}

public class GetVisitHistoryQueryHandler(IVisitRepository repository) : IQueryHandler<GetVisitHistoryQuery, IEnumerable<VisitWithTariffDto>>
{
    private readonly IVisitRepository _repository = repository;

    public async Task<Result<IEnumerable<VisitWithTariffDto>>> Handle(GetVisitHistoryQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var visits = await _repository.GetVisitHistoryByUserAsync(request.UserId, request.PageNumber, request.PageSize);
            return Result.Ok(visits);
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error("Внутренняя ошибка").CausedBy(ex));
        }
    }
}

