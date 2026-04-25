namespace Venue.TimeCafe.Application.CQRS.Visits.Queries;

public record GetActiveVisitsQuery() : IQuery<IEnumerable<VisitWithTariffDto>>;

public class GetActiveVisitsQueryValidator : AbstractValidator<GetActiveVisitsQuery>
{
    public GetActiveVisitsQueryValidator()
    {
    }
}


public class GetActiveVisitsQueryHandler(IVisitRepository repository) : IQueryHandler<GetActiveVisitsQuery, IEnumerable<VisitWithTariffDto>>
{
    private readonly IVisitRepository _repository = repository;

    public async Task<Result<IEnumerable<VisitWithTariffDto>>> Handle(GetActiveVisitsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var visits = await _repository.GetActiveVisitsAsync();
            return Result.Ok(visits);
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error("Внутренняя ошибка").CausedBy(ex));
        }
    }
}

