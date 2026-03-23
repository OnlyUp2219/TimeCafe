namespace Venue.TimeCafe.Application.CQRS.Promotions.Commands;

public record DeletePromotionCommand(Guid PromotionId) : IRequest<DeletePromotionResult>;

public record DeletePromotionResult(
    bool Success,
    string? Code = null,
    string? Message = null,
    int? StatusCode = null,
    List<ErrorItem>? Errors = null) : ICqrsResultV2
{
    public static DeletePromotionResult PromotionNotFound() =>
        new(false, Code: "PromotionNotFound", Message: "Акция не найдена", StatusCode: 404);

    public static DeletePromotionResult DeleteFailed() =>
        new(false, Code: "DeletePromotionFailed", Message: "Не удалось удалить акцию", StatusCode: 500);

    public static DeletePromotionResult DeleteSuccess() =>
        new(true, Message: "Акция успешно удалена");
}

public class DeletePromotionCommandValidator : AbstractValidator<DeletePromotionCommand>
{
    public DeletePromotionCommandValidator()
    {
        RuleFor(x => x.PromotionId).ValidGuidEntityId("Акция не найдена");
    }
}

public class DeletePromotionCommandHandler(IPromotionRepository repository) : IRequestHandler<DeletePromotionCommand, DeletePromotionResult>
{
    private readonly IPromotionRepository _repository = repository;

    public async Task<DeletePromotionResult> Handle(DeletePromotionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var existing = await _repository.GetByIdAsync(request.PromotionId);
            if (existing == null)
                return DeletePromotionResult.PromotionNotFound();

            var result = await _repository.DeleteAsync(request.PromotionId);

            if (!result)
                return DeletePromotionResult.DeleteFailed();

            return DeletePromotionResult.DeleteSuccess();
        }
        catch (Exception ex)
        {
            throw new CqrsResultException(DeletePromotionResult.DeleteFailed(), ex);
        }
    }
}
