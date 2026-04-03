namespace Venue.TimeCafe.Application.CQRS.Promotions.Commands;

public record DeactivatePromotionCommand(Guid PromotionId) : IRequest<DeactivatePromotionResult>;

public record DeactivatePromotionResult(
    bool Success,
    string? Code = null,
    string? Message = null,
    int? StatusCode = null,
    List<ErrorItem>? Errors = null) : ICqrsResult
{
    public static DeactivatePromotionResult PromotionNotFound() =>
        new(false, Code: "PromotionNotFound", Message: "Акция не найдена", StatusCode: 404);

    public static DeactivatePromotionResult DeactivateFailed() =>
        new(false, Code: "DeactivatePromotionFailed", Message: "Не удалось деактивировать акцию", StatusCode: 500);

    public static DeactivatePromotionResult DeactivateSuccess() =>
        new(true, Message: "Акция успешно деактивирована");
}

public class DeactivatePromotionCommandValidator : AbstractValidator<DeactivatePromotionCommand>
{
    public DeactivatePromotionCommandValidator()
    {
        RuleFor(x => x.PromotionId).ValidGuidEntityId("Акция не найдена");
    }
}

public class DeactivatePromotionCommandHandler(IPromotionRepository repository) : IRequestHandler<DeactivatePromotionCommand, DeactivatePromotionResult>
{
    private readonly IPromotionRepository _repository = repository;

    public async Task<DeactivatePromotionResult> Handle(DeactivatePromotionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var existing = await _repository.GetByIdAsync(request.PromotionId);
            if (existing == null)
                return DeactivatePromotionResult.PromotionNotFound();

            var result = await _repository.DeactivateAsync(request.PromotionId);

            if (!result)
                return DeactivatePromotionResult.DeactivateFailed();

            return DeactivatePromotionResult.DeactivateSuccess();
        }
        catch (Exception ex)
        {
            throw new CqrsResultException(DeactivatePromotionResult.DeactivateFailed(), ex);
        }
    }
}
