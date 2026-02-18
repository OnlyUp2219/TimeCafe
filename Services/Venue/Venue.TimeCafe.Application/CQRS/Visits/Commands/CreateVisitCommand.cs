namespace Venue.TimeCafe.Application.CQRS.Visits.Commands;

public record CreateVisitCommand(
    string UserId,
    string TariffId,
    int? PlannedMinutes = null,
    bool RequirePositiveBalance = true,
    bool RequireEnoughForPlanned = false) : IRequest<CreateVisitResult>;

public record CreateVisitResult(
    bool Success,
    string? Code = null,
    string? Message = null,
    int? StatusCode = null,
    List<ErrorItem>? Errors = null,
    Visit? Visit = null) : ICqrsResultV2
{
    public static CreateVisitResult TariffNotFound() =>
        new(false, Code: "TariffNotFound", Message: "Тариф не найден", StatusCode: 404);
    public static CreateVisitResult InsufficientFunds(string message) =>
        new(false, Code: "InsufficientFunds", Message: message, StatusCode: 400);
    public static CreateVisitResult BalanceCheckFailed() =>
        new(false, Code: "BalanceCheckFailed", Message: "Не удалось проверить баланс. Повторите попытку позже", StatusCode: 503);
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
            .NotNull().WithMessage("Пользователь не найден")
            .Must(x => Guid.TryParse(x, out var guid) && guid != Guid.Empty).WithMessage("Пользователь не найден");

        RuleFor(x => x.TariffId)
            .NotEmpty().WithMessage("Тариф не найден")
            .NotNull().WithMessage("Тариф не найден")
            .Must(x => Guid.TryParse(x, out var guid) && guid != Guid.Empty).WithMessage("Тариф не найден");

        RuleFor(x => x.PlannedMinutes)
            .GreaterThan(0).WithMessage("Время посещения должно быть больше 0 минут")
            .LessThanOrEqualTo(12 * 60).WithMessage("Максимальное планируемое время — 12 часов")
            .When(x => x.PlannedMinutes.HasValue);
    }
}

public class CreateVisitCommandHandler(
    IVisitRepository repository,
    ITariffRepository tariffRepository,
    IVisitBalancePolicyService visitBalancePolicyService) : IRequestHandler<CreateVisitCommand, CreateVisitResult>
{
    private readonly IVisitRepository _repository = repository;
    private readonly ITariffRepository _tariffRepository = tariffRepository;
    private readonly IVisitBalancePolicyService _visitBalancePolicyService = visitBalancePolicyService;

    public async Task<CreateVisitResult> Handle(CreateVisitCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var userId = Guid.Parse(request.UserId);
            var tariffId = Guid.Parse(request.TariffId);

            var tariff = await _tariffRepository.GetByIdAsync(tariffId);
            if (tariff == null)
                return CreateVisitResult.TariffNotFound();

            if (request.RequirePositiveBalance || request.RequireEnoughForPlanned)
            {
                var balanceCheck = await _visitBalancePolicyService.CheckBeforeStartAsync(
                    userId,
                    tariff,
                    request.PlannedMinutes,
                    request.RequirePositiveBalance,
                    request.RequireEnoughForPlanned,
                    cancellationToken);

                if (!balanceCheck.IsAllowed)
                {
                    return CreateVisitResult.InsufficientFunds(
                        balanceCheck.Message ?? "Недостаточно средств для старта визита");
                }
            }

            var visit = new Visit
            {
                UserId = userId,
                TariffId = tariffId,
                EntryTime = DateTimeOffset.UtcNow,
                Status = VisitStatus.Active
            };

            var created = await _repository.CreateAsync(visit);

            if (created == null)
                return CreateVisitResult.CreateFailed();

            return CreateVisitResult.CreateSuccess(created);
        }
        catch (HttpRequestException)
        {
            return CreateVisitResult.BalanceCheckFailed();
        }
        catch (TaskCanceledException)
        {
            return CreateVisitResult.BalanceCheckFailed();
        }
        catch (Exception ex)
        {
            throw new CqrsResultException(CreateVisitResult.CreateFailed(), ex);
        }
    }
}
