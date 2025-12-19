using Venue.TimeCafe.Application.Contracts.Repositories;

namespace Venue.TimeCafe.Application.CQRS.Promotions.Commands;

public record DeactivatePromotionCommand(string PromotionId) : IRequest<DeactivatePromotionResult>;

public record DeactivatePromotionResult(
    bool Success,
    string? Code = null,
    string? Message = null,
    int? StatusCode = null,
    List<ErrorItem>? Errors = null) : ICqrsResultV2
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
        RuleFor(x => x.PromotionId)
            .NotEmpty().WithMessage("Акция не найдена")
            .Must(x => !string.IsNullOrWhiteSpace(x)).WithMessage("Акция не найдена")
            .Must(x => Guid.TryParse(x, out var guid) && guid != Guid.Empty).WithMessage("Акция не найдена");
    }
}

public class DeactivatePromotionCommandHandler(IPromotionRepository repository) : IRequestHandler<DeactivatePromotionCommand, DeactivatePromotionResult>
{
    private readonly IPromotionRepository _repository = repository;

    public async Task<DeactivatePromotionResult> Handle(DeactivatePromotionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var promotionId = Guid.Parse(request.PromotionId);


            var existing = await _repository.GetByIdAsync(promotionId);
            if (existing == null)
                return DeactivatePromotionResult.PromotionNotFound();

            var result = await _repository.DeactivateAsync(promotionId);

            if (!result)
                return DeactivatePromotionResult.DeactivateFailed();

            return DeactivatePromotionResult.DeactivateSuccess();
        }
        catch (Exception)
        {
            return DeactivatePromotionResult.DeactivateFailed();
        }
    }
}
