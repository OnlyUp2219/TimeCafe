namespace Venue.TimeCafe.Application.CQRS.Visits.Queries;

public record HasActiveVisitQuery(string UserId) : IRequest<HasActiveVisitResult>;

public record HasActiveVisitResult(
    bool Success,
    string? Code = null,
    string? Message = null,
    int? StatusCode = null,
    List<ErrorItem>? Errors = null,
    bool HasActiveVisit = false) : ICqrsResultV2
{
    public static HasActiveVisitResult CheckFailed() =>
        new(false, Code: "CheckActiveVisitFailed", Message: "Не удалось проверить активное посещение", StatusCode: 500);

    public static HasActiveVisitResult CheckSuccess(bool hasActiveVisit) =>
        new(true, HasActiveVisit: hasActiveVisit);
}

public class HasActiveVisitQueryValidator : AbstractValidator<HasActiveVisitQuery>
{
    public HasActiveVisitQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("ID пользователя обязателен")
            .MaximumLength(450).WithMessage("ID пользователя не может превышать 450 символов");
    }
}

public class HasActiveVisitQueryHandler(IVisitRepository repository) : IRequestHandler<HasActiveVisitQuery, HasActiveVisitResult>
{
    private readonly IVisitRepository _repository = repository;

    public async Task<HasActiveVisitResult> Handle(HasActiveVisitQuery request, CancellationToken cancellationToken)
    {
        try
        {
            Guid userId = Guid.Parse(request.UserId);

            var hasActiveVisit = await _repository.HasActiveVisitAsync(userId);
            return HasActiveVisitResult.CheckSuccess(hasActiveVisit);
        }
        catch (Exception ex)
        {
            throw new CqrsResultException(HasActiveVisitResult.CheckFailed(), ex);
        }
    }
}
