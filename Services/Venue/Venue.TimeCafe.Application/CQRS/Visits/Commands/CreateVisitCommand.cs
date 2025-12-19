using Venue.TimeCafe.Application.Contracts.Repositories;

namespace Venue.TimeCafe.Application.CQRS.Visits.Commands;

public record CreateVisitCommand(string UserId, string TariffId) : IRequest<CreateVisitResult>;

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
            .NotEmpty().WithMessage("Пользователь не найден")
            .Must(x => !string.IsNullOrWhiteSpace(x)).WithMessage("Пользователь не найден")
            .Must(x => Guid.TryParse(x, out var guid) && guid != Guid.Empty).WithMessage("Пользователь не найден");

        RuleFor(x => x.TariffId)
            .NotEmpty().WithMessage("Тариф не найден")
            .Must(x => !string.IsNullOrWhiteSpace(x)).WithMessage("Тариф не найден")
            .Must(x => Guid.TryParse(x, out var guid) && guid != Guid.Empty).WithMessage("Тариф не найден");
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
                UserId = Guid.Parse(request.UserId),
                TariffId = Guid.Parse(request.TariffId),
                EntryTime = DateTimeOffset.UtcNow,
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
