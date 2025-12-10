namespace Venue.TimeCafe.Application.CQRS.Promotions.Commands;

public record DeletePromotionCommand(string PromotionId) : IRequest<DeletePromotionResult>;

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
        RuleFor(x => x.PromotionId)
    .NotEmpty().WithMessage("Акция не найдена")
    .Must(x => !string.IsNullOrWhiteSpace(x)).WithMessage("Акция не найдена")
    .Must(x => Guid.TryParse(x, out _)).WithMessage("Акция не найдена");
    }
}

public class DeletePromotionCommandHandler(IPromotionRepository repository) : IRequestHandler<DeletePromotionCommand, DeletePromotionResult>
{
    private readonly IPromotionRepository _repository = repository;

    public async Task<DeletePromotionResult> Handle(DeletePromotionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var promotionId = Guid.Parse(request.PromotionId);

            var existing = await _repository.GetByIdAsync(promotionId);
            if (existing == null)
                return DeletePromotionResult.PromotionNotFound();

            var result = await _repository.DeleteAsync(promotionId);

            if (!result)
                return DeletePromotionResult.DeleteFailed();

            return DeletePromotionResult.DeleteSuccess();
        }
        catch (Exception)
        {
            return DeletePromotionResult.DeleteFailed();
        }
    }
}
