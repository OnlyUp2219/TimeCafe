namespace Venue.TimeCafe.Application.CQRS.Visits.Queries;

public record GetActiveVisitByUserQuery(Guid UserId) : IQuery<VisitWithTariffDto>;

public class GetActiveVisitByUserQueryValidator : AbstractValidator<GetActiveVisitByUserQuery>
{
    public GetActiveVisitByUserQueryValidator()
    {
        RuleFor(x => x.UserId).ValidGuidEntityId("Пользователь не найден");
    }
}

public class GetActiveVisitByUserQueryHandler(IVisitRepository repository) : IQueryHandler<GetActiveVisitByUserQuery, VisitWithTariffDto>
{
    private readonly IVisitRepository _repository = repository;

    public async Task<Result<VisitWithTariffDto>> Handle(GetActiveVisitByUserQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var visit = await _repository.GetActiveVisitByUserAsync(request.UserId);

            if (visit == null)
                return Result.Fail(new VisitNotFoundError());

            return Result.Ok(visit);
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error("Внутренняя ошибка").CausedBy(ex));
        }
    }
}

