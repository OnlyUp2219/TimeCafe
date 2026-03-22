namespace BuildingBlocks.Validation;

public static class PromotionValidationExtensions
{
    public static IRuleBuilderOptions<T, decimal?> ValidDiscountPercent<T>(
        this IRuleBuilder<T, decimal?> ruleBuilder)
    {
        return ruleBuilder
            .GreaterThan(0).WithMessage("Процент скидки должен быть больше 0")
            .LessThanOrEqualTo(100).WithMessage("Процент скидки не может превышать 100");
    }

    public static IRuleBuilderOptions<T, DateTimeOffset> ValidFromBeforeValidTo<T>(
        this IRuleBuilder<T, DateTimeOffset> ruleBuilder, Func<T, DateTimeOffset> validToSelector)
    {
        return ruleBuilder
            .Must((cmd, validFrom) => validFrom < validToSelector(cmd))
            .WithMessage("Дата начала должна быть раньше даты окончания");
    }
}
