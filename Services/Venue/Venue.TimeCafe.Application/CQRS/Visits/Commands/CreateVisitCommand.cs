namespace Venue.TimeCafe.Application.CQRS.Visits.Commands;

public record CreateVisitCommand(string UserId, int TariffId) : IRequest<CreateVisitResult>;

public record CreateVisitResult(
    bool Success,
    string? Code = null,
    string? Message = null,
    int? StatusCode = null,
    List<ErrorItem>? Errors = null,
    Visit? Visit = null) : ICqrsResultV2
{
    public static CreateVisitResult CreateFailed() =>
        new(false, Code: "CreateVisitFailed", Message: "Не удалось создать посещение", StatusCode: 500);

    public static CreateVisitResult CreateSuccess(Visit visit) =>
        new(true, Message: "Посещение успешно создано", StatusCode: 201, Visit: visit);
}

public class CreateVisitCommandValidator : AbstractValidator<CreateVisitCommand>
{
    public CreateVisitCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("ID пользователя обязателен")
            .MaximumLength(450).WithMessage("ID пользователя не может превышать 450 символов");

        RuleFor(x => x.TariffId)
            .GreaterThan(0).WithMessage("ID тарифа обязателен");
    }
}

public class CreateVisitCommandHandler(IVisitRepository repository) : IRequestHandler<CreateVisitCommand, CreateVisitResult>
{
    private readonly IVisitRepository _repository = repository;

    public async Task<CreateVisitResult> Handle(CreateVisitCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var visit = new Visit
            {
                UserId = request.UserId,
                TariffId = request.TariffId,
                EntryTime = DateTime.UtcNow,
                Status = VisitStatus.Active
            };

            var created = await _repository.CreateAsync(visit);

            if (created == null)
                return CreateVisitResult.CreateFailed();

            return CreateVisitResult.CreateSuccess(created);
        }
        catch (Exception)
        {
            return CreateVisitResult.CreateFailed();
        }
    }
}
