namespace Venue.TimeCafe.Application.CQRS.Visits.Queries;

public record GetVisitByIdQuery(string VisitId) : IRequest<GetVisitByIdResult>;

public record GetVisitByIdResult(
    bool Success,
    string? Code = null,
    string? Message = null,
    int? StatusCode = null,
    List<ErrorItem>? Errors = null,
    VisitWithTariffDto? Visit = null) : ICqrsResultV2
{
    public static GetVisitByIdResult VisitNotFound() =>
        new(false, Code: "VisitNotFound", Message: "Посещение не найдено", StatusCode: 404);

    public static GetVisitByIdResult GetFailed() =>
        new(false, Code: "GetVisitFailed", Message: "Не удалось получить посещение", StatusCode: 500);

    public static GetVisitByIdResult GetSuccess(VisitWithTariffDto visit) =>
        new(true, Visit: visit);
}

public class GetVisitByIdQueryValidator : AbstractValidator<GetVisitByIdQuery>
{
    public GetVisitByIdQueryValidator()
    {
        RuleFor(x => x.VisitId)
            .NotEmpty().WithMessage("Посещение не найдено")
            .Must(x => !string.IsNullOrWhiteSpace(x)).WithMessage("Посещение не найдено")
            .Must(x => Guid.TryParse(x, out var guid) && guid != Guid.Empty).WithMessage("Посещение не найдено");
    }
}

public class GetVisitByIdQueryHandler(IVisitRepository repository) : IRequestHandler<GetVisitByIdQuery, GetVisitByIdResult>
{
    private readonly IVisitRepository _repository = repository;

    public async Task<GetVisitByIdResult> Handle(GetVisitByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var visitId = Guid.Parse(request.VisitId);

            var visit = await _repository.GetByIdAsync(visitId);

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
