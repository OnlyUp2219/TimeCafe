namespace Venue.TimeCafe.Application.CQRS.Visits.Queries;

public record GetVisitByIdQuery(int VisitId) : IRequest<GetVisitByIdResult>;

public record GetVisitByIdResult(
    bool Success,
    string? Code = null,
    string? Message = null,
    int? StatusCode = null,
    List<ErrorItem>? Errors = null,
    Visit? Visit = null) : ICqrsResultV2
{
    public static GetVisitByIdResult VisitNotFound() =>
        new(false, Code: "VisitNotFound", Message: "Посещение не найдено", StatusCode: 404);

    public static GetVisitByIdResult GetFailed() =>
        new(false, Code: "GetVisitFailed", Message: "Не удалось получить посещение", StatusCode: 500);

    public static GetVisitByIdResult GetSuccess(Visit visit) =>
        new(true, Visit: visit);
}

public class GetVisitByIdQueryValidator : AbstractValidator<GetVisitByIdQuery>
{
    public GetVisitByIdQueryValidator()
    {
        RuleFor(x => x.VisitId)
            .GreaterThan(0).WithMessage("ID посещения обязателен");
    }
}

public class GetVisitByIdQueryHandler(IVisitRepository repository) : IRequestHandler<GetVisitByIdQuery, GetVisitByIdResult>
{
    private readonly IVisitRepository _repository = repository;

    public async Task<GetVisitByIdResult> Handle(GetVisitByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var visit = await _repository.GetByIdAsync(request.VisitId);

            if (visit == null)
                return GetVisitByIdResult.VisitNotFound();

            return GetVisitByIdResult.GetSuccess(visit);
        }
        catch (Exception)
        {
            return GetVisitByIdResult.GetFailed();
        }
    }
}
