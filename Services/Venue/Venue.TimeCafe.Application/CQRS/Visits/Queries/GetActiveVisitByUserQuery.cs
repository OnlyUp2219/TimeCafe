namespace Venue.TimeCafe.Application.CQRS.Visits.Queries;

public record GetActiveVisitByUserQuery(Guid UserId) : IQuery<VisitWithTariffDto>;

public class GetActiveVisitByUserQueryHandler(IUnitOfWork uow) : IQueryHandler<GetActiveVisitByUserQuery, VisitWithTariffDto>
{
    private readonly IUnitOfWork _uow = uow;

    public async Task<Result<VisitWithTariffDto>> Handle(GetActiveVisitByUserQuery request, CancellationToken cancellationToken = default)
    {
        try
        {
            var visit = await _uow.Visits.GetActiveVisitByUserAsync(request.UserId, cancellationToken);

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

