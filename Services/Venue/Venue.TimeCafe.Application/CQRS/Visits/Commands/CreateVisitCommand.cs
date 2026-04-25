namespace Venue.TimeCafe.Application.CQRS.Visits.Commands;

public record CreateVisitCommand(
    Guid UserId,
    Guid TariffId,
    int? PlannedMinutes = null,
    bool RequirePositiveBalance = true,
    bool RequireEnoughForPlanned = false) : ICommand<Visit>;

public class CreateVisitCommandValidator : AbstractValidator<CreateVisitCommand>
{
    public CreateVisitCommandValidator()
    {
        RuleFor(x => x.UserId).ValidGuidEntityId("Пользователь не найден");

        RuleFor(x => x.TariffId).ValidGuidEntityId("Тариф не найден");

        RuleFor(x => x.PlannedMinutes).ValidPlannedMinutes()
            .When(x => x.PlannedMinutes.HasValue);
    }
}

public class CreateVisitCommandHandler(
    IVisitRepository repository,
    ITariffRepository tariffRepository,
    IVisitBalancePolicyService visitBalancePolicyService) : ICommandHandler<CreateVisitCommand, Visit>
{
    private readonly IVisitRepository _repository = repository;
    private readonly ITariffRepository _tariffRepository = tariffRepository;
    private readonly IVisitBalancePolicyService _visitBalancePolicyService = visitBalancePolicyService;

    public async Task<Result<Visit>> Handle(CreateVisitCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var tariff = await _tariffRepository.GetByIdAsync(request.TariffId);
            if (tariff == null)
                return Result.Fail(new TariffNotFoundError());

            if (request.RequirePositiveBalance || request.RequireEnoughForPlanned)
            {
                var balanceCheck = await _visitBalancePolicyService.CheckBeforeStartAsync(
                    request.UserId,
                    tariff,
                    request.PlannedMinutes,
                    request.RequirePositiveBalance,
                    request.RequireEnoughForPlanned,
                    cancellationToken);

                if (!balanceCheck.IsAllowed)
                {
                    return Result.Fail(new InsufficientFundsError());
                }
            }

            var visit = new Visit
            {
                UserId = request.UserId,
                TariffId = request.TariffId,
                EntryTime = DateTimeOffset.UtcNow,
                Status = VisitStatus.Active
            };

            var created = await _repository.CreateAsync(visit);

            if (created == null)
                return Result.Fail(new CreateFailedError());

            return Result.Ok(created);
        }
        catch (HttpRequestException)
        {
            return Result.Fail(new BalanceCheckFailedError());
        }
        catch (TaskCanceledException)
        {
            return Result.Fail(new BalanceCheckFailedError());
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error("Внутренняя ошибка").CausedBy(ex));
        }
    }
}

