namespace Venue.TimeCafe.Application.CQRS.Visits.Queries;

public record GetVisitByIdQuery(Guid VisitId) : IQuery<VisitWithTariffDto>;

public class GetVisitByIdQueryHandler(IUnitOfWork uow) : IQueryHandler<GetVisitByIdQuery, VisitWithTariffDto>
{
    private readonly IUnitOfWork _uow = uow;

    public async Task<Result<VisitWithTariffDto>> Handle(GetVisitByIdQuery request, CancellationToken cancellationToken = default)
    {
        try
        {
            var visit = await _uow.Visits.GetWithTariffByIdAsync(request.VisitId, cancellationToken);

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

