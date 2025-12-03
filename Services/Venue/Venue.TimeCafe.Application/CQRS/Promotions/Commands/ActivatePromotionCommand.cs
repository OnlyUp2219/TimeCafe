namespace Venue.TimeCafe.Application.CQRS.Promotions.Commands;

public record ActivatePromotionCommand(int PromotionId) : IRequest<ActivatePromotionResult>;

public record ActivatePromotionResult(
    bool Success,
    string? Code = null,
    string? Message = null,
    int? StatusCode = null,
    List<ErrorItem>? Errors = null) : ICqrsResultV2
{
    public static ActivatePromotionResult PromotionNotFound() =>
        new(false, Code: "PromotionNotFound", Message: "Акция не найдена", StatusCode: 404);

    public static ActivatePromotionResult ActivateFailed() =>
        new(false, Code: "ActivatePromotionFailed", Message: "Не удалось активировать акцию", StatusCode: 500);

    public static ActivatePromotionResult ActivateSuccess() =>
        new(true, Message: "Акция успешно активирована");
}

public class ActivatePromotionCommandValidator : AbstractValidator<ActivatePromotionCommand>
{
    public ActivatePromotionCommandValidator()
    {
        RuleFor(x => x.PromotionId)
            .GreaterThan(0).WithMessage("ID акции обязателен");
    }
}

public class ActivatePromotionCommandHandler(IPromotionRepository repository) : IRequestHandler<ActivatePromotionCommand, ActivatePromotionResult>
{
    private readonly IPromotionRepository _repository = repository;

    public async Task<ActivatePromotionResult> Handle(ActivatePromotionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var existing = await _repository.GetByIdAsync(request.PromotionId);
            if (existing == null)
                return ActivatePromotionResult.PromotionNotFound();

            var result = await _repository.ActivateAsync(request.PromotionId);

            if (!result)
                return ActivatePromotionResult.ActivateFailed();

            return ActivatePromotionResult.ActivateSuccess();
        }
        catch (Exception)
        {
            return ActivatePromotionResult.ActivateFailed();
        }
    }
}
