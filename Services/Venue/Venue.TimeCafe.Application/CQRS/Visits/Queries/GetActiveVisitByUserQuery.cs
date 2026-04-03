namespace Venue.TimeCafe.Application.CQRS.Visits.Queries;

public record GetActiveVisitByUserQuery(Guid UserId) : IRequest<GetActiveVisitByUserResult>;

public record GetActiveVisitByUserResult(
    bool Success,
    string? Code = null,
    string? Message = null,
    int? StatusCode = null,
    List<ErrorItem>? Errors = null,
    VisitWithTariffDto? Visit = null) : ICqrsResult
{
    public static GetActiveVisitByUserResult VisitNotFound() =>
        new(false, Code: "ActiveVisitNotFound", Message: "Активное посещение не найдено", StatusCode: 404);

    public static GetActiveVisitByUserResult GetFailed() =>
        new(false, Code: "GetActiveVisitFailed", Message: "Не удалось получить активное посещение", StatusCode: 500);

    public static GetActiveVisitByUserResult GetSuccess(VisitWithTariffDto visit) =>
        new(true, Visit: visit);
}

public class GetActiveVisitByUserQueryValidator : AbstractValidator<GetActiveVisitByUserQuery>
{
    public GetActiveVisitByUserQueryValidator()
    {
        RuleFor(x => x.UserId).ValidGuidEntityId("Пользователь не найден");
    }
}

public class GetActiveVisitByUserQueryHandler(IVisitRepository repository) : IRequestHandler<GetActiveVisitByUserQuery, GetActiveVisitByUserResult>
{
    private readonly IVisitRepository _repository = repository;

    public async Task<GetActiveVisitByUserResult> Handle(GetActiveVisitByUserQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var visit = await _repository.GetActiveVisitByUserAsync(request.UserId);

            if (visit == null)
                return GetActiveVisitByUserResult.VisitNotFound();

            return GetActiveVisitByUserResult.GetSuccess(visit);
        }
        catch (Exception ex)
        {
            throw new CqrsResultException(GetActiveVisitByUserResult.GetFailed(), ex);
        }
    }
}
