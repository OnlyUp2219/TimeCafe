namespace Venue.TimeCafe.Application.CQRS.Visits.Commands;

public record CreateVisitCommand(
    Guid UserId,
    Guid TariffId,
    int? PlannedMinutes = null,
    int GuestsCount = 1,
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
            
        RuleFor(x => x.GuestsCount).GreaterThan(0).WithMessage("Количество гостей должно быть больше нуля");
    }
}

public class CreateVisitCommandHandler(IUnitOfWork uow, IVisitBalancePolicyService visitBalancePolicyService, IPublisher publisher) : ICommandHandler<CreateVisitCommand, Visit>
{
    private readonly IUnitOfWork _uow = uow;
    private readonly IVisitBalancePolicyService _visitBalancePolicyService = visitBalancePolicyService;
    private readonly IPublisher _publisher = publisher;

    public async Task<Result<Visit>> Handle(CreateVisitCommand request, CancellationToken cancellationToken = default)
    {
        try
        {
            var tariff = await _uow.Tariffs.GetWithThemeByIdAsync(request.TariffId, cancellationToken);
            if (tariff == null)
                return Result.Fail(new TariffNotFoundError());

            if (tariff.MaxGuests.HasValue && request.GuestsCount > tariff.MaxGuests.Value)
                return Result.Fail(new Error("Превышено максимальное количество гостей для данного тарифа"));

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

            var created = await _uow.Visits.CreateAsync(visit, cancellationToken);

            if (created == null)
                return Result.Fail(new CreateFailedError());

            await _uow.SaveChangesAsync(cancellationToken);
            await _publisher.Publish(new VisitChangedEvent(created.VisitId, created.UserId), cancellationToken);

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

