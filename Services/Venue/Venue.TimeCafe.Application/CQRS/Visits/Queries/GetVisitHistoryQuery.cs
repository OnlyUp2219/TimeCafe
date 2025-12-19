using Venue.TimeCafe.Application.Contracts.Repositories;

namespace Venue.TimeCafe.Application.CQRS.Visits.Queries;

public record GetVisitHistoryQuery(string UserId, int PageNumber, int PageSize) : IRequest<GetVisitHistoryResult>;

public record GetVisitHistoryResult(
    bool Success,
    string? Code = null,
    string? Message = null,
    int? StatusCode = null,
    List<ErrorItem>? Errors = null,
    IEnumerable<VisitWithTariffDto>? Visits = null) : ICqrsResultV2
{
    public static GetVisitHistoryResult GetFailed() =>
        new(false, Code: "GetVisitHistoryFailed", Message: "Не удалось получить историю посещений", StatusCode: 500);

    public static GetVisitHistoryResult GetSuccess(IEnumerable<VisitWithTariffDto> visits) =>
        new(true, Visits: visits);
}

public class GetVisitHistoryQueryValidator : AbstractValidator<GetVisitHistoryQuery>
{
    public GetVisitHistoryQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Пользователь не найден")
            .Must(x => !string.IsNullOrWhiteSpace(x)).WithMessage("Пользователь не найден")
            .Must(x => Guid.TryParse(x, out var guid) && guid != Guid.Empty).WithMessage("Пользователь не найден");

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
            Guid userId = Guid.Parse(request.UserId);

            var visits = await _repository.GetVisitHistoryByUserAsync(userId, request.PageNumber, request.PageSize);
            return GetVisitHistoryResult.GetSuccess(visits);
        }
        catch (Exception)
        {
            return GetVisitHistoryResult.GetFailed();
        }
    }
}
