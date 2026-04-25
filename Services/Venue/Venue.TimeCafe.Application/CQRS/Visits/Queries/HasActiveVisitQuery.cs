namespace Venue.TimeCafe.Application.CQRS.Visits.Queries;

public record HasActiveVisitQuery(Guid UserId) : IQuery<bool>;

public class HasActiveVisitQueryValidator : AbstractValidator<HasActiveVisitQuery>
{
    public HasActiveVisitQueryValidator()
    {
        RuleFor(x => x.UserId).ValidGuidEntityId("Пользователь не найден");
    }
}

public class HasActiveVisitQueryHandler(IVisitRepository repository) : IQueryHandler<HasActiveVisitQuery, bool>
{
    private readonly IVisitRepository _repository = repository;

    public async Task<Result<bool>> Handle(HasActiveVisitQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var hasActiveVisit = await _repository.HasActiveVisitAsync(request.UserId);
            return Result.Ok(hasActiveVisit);
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error("Внутренняя ошибка").CausedBy(ex));
        }
    }
}

