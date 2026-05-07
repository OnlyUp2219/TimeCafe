namespace Venue.TimeCafe.Application.CQRS.Visits.Queries;

public record HasActiveVisitQuery(Guid UserId) : IQuery<bool>;

public class HasActiveVisitQueryHandler(IUnitOfWork uow) : IQueryHandler<HasActiveVisitQuery, bool>
{
    private readonly IUnitOfWork _uow = uow;

    public async Task<Result<bool>> Handle(HasActiveVisitQuery request, CancellationToken cancellationToken = default)
    {
        try
        {
            var hasActiveVisit = await _uow.Visits.HasActiveVisitAsync(request.UserId, cancellationToken);
            return Result.Ok(hasActiveVisit);
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error("Внутренняя ошибка").CausedBy(ex));
        }
    }
}

