namespace Venue.TimeCafe.Application.CQRS.Promotions.Commands;

public record ActivatePromotionCommand(Guid PromotionId) : ICommand;

public class ActivatePromotionCommandValidator : AbstractValidator<ActivatePromotionCommand>
{
    public ActivatePromotionCommandValidator()
    {
        RuleFor(x => x.PromotionId).ValidGuidEntityId("Акция не найдена");
    }
}

public class ActivatePromotionCommandHandler(IPromotionRepository repository) : ICommandHandler<ActivatePromotionCommand>
{
    private readonly IPromotionRepository _repository = repository;

    public async Task<Result> Handle(ActivatePromotionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var existing = await _repository.GetByIdAsync(request.PromotionId);
            if (existing == null)
                return Result.Fail(new PromotionNotFoundError());

            var result = await _repository.ActivateAsync(request.PromotionId);

            if (!result)
                return Result.Fail(new ActivateFailedError());

            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error("Внутренняя ошибка").CausedBy(ex));
        }
    }
}

