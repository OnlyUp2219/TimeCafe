namespace Venue.TimeCafe.Application.CQRS.Promotions.Commands;

public record DeletePromotionCommand(Guid PromotionId) : ICommand;

public class DeletePromotionCommandValidator : AbstractValidator<DeletePromotionCommand>
{
    public DeletePromotionCommandValidator()
    {
        RuleFor(x => x.PromotionId).ValidGuidEntityId("Акция не найдена");
    }
}

public class DeletePromotionCommandHandler(IPromotionRepository repository) : ICommandHandler<DeletePromotionCommand>
{
    private readonly IPromotionRepository _repository = repository;

    public async Task<Result> Handle(DeletePromotionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var existing = await _repository.GetByIdAsync(request.PromotionId);
            if (existing == null)
                return Result.Fail(new PromotionNotFoundError());

            var result = await _repository.DeleteAsync(request.PromotionId);

            if (!result)
                return Result.Fail(new DeleteFailedError());

            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error("Внутренняя ошибка").CausedBy(ex));
        }
    }
}

