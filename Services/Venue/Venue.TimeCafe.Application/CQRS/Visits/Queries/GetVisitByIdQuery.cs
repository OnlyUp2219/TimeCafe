namespace Venue.TimeCafe.Application.CQRS.Visits.Queries;

public record GetVisitByIdQuery(Guid VisitId) : IQuery<VisitWithTariffDto>;

public class GetVisitByIdQueryValidator : AbstractValidator<GetVisitByIdQuery>
{
    public GetVisitByIdQueryValidator()
    {
        RuleFor(x => x.VisitId).ValidGuidEntityId("Посещение не найдено");
    }
}

public class GetVisitByIdQueryHandler(IVisitRepository repository) : IQueryHandler<GetVisitByIdQuery, VisitWithTariffDto>
{
    private readonly IVisitRepository _repository = repository;

    public async Task<Result<VisitWithTariffDto>> Handle(GetVisitByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var visit = await _repository.GetByIdAsync(request.VisitId);

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

