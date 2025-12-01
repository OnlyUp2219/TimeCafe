namespace Venue.TimeCafe.Application.CQRS.Visits.Queries;

public record GetVisitHistoryQuery(string UserId, int PageNumber, int PageSize) : IRequest<GetVisitHistoryResult>;

public record GetVisitHistoryResult(
    bool Success,
    string? Code = null,
    string? Message = null,
    int? StatusCode = null,
    List<ErrorItem>? Errors = null,
    IEnumerable<Visit>? Visits = null) : ICqrsResultV2
{
    public static GetVisitHistoryResult GetFailed() =>
        new(false, Code: "GetVisitHistoryFailed", Message: "Не удалось получить историю посещений", StatusCode: 500);

    public static GetVisitHistoryResult GetSuccess(IEnumerable<Visit> visits) =>
        new(true, Visits: visits);
}

public class GetVisitHistoryQueryValidator : AbstractValidator<GetVisitHistoryQuery>
{
    public GetVisitHistoryQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("ID пользователя обязателен")
            .MaximumLength(450).WithMessage("ID пользователя не может превышать 450 символов");

        RuleFor(x => x.PageNumber)
            .GreaterThan(0).WithMessage("Номер страницы должен быть больше 0");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("Размер страницы должен быть больше 0")
            .LessThanOrEqualTo(100).WithMessage("Размер страницы не может превышать 100");
    }
}

public class GetVisitHistoryQueryHandler(IVisitRepository repository) : IRequestHandler<GetVisitHistoryQuery, GetVisitHistoryResult>
{
    private readonly IVisitRepository _repository = repository;

    public async Task<GetVisitHistoryResult> Handle(GetVisitHistoryQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var visits = await _repository.GetVisitHistoryByUserAsync(request.UserId, request.PageNumber, request.PageSize);
            return GetVisitHistoryResult.GetSuccess(visits);
        }
        catch (Exception)
        {
            return GetVisitHistoryResult.GetFailed();
        }
    }
}
