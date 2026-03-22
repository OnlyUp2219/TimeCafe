namespace BuildingBlocks.Validation;

public static class VisitValidationExtensions
{
    public static IRuleBuilderOptions<T, int?> ValidPlannedMinutes<T>(
        this IRuleBuilder<T, int?> ruleBuilder, int maxMinutes = 720)
    {
        return ruleBuilder
            .GreaterThan(0).WithMessage("Время посещения должно быть больше 0 минут")
            .LessThanOrEqualTo(maxMinutes).WithMessage($"Максимальное планируемое время — {maxMinutes / 60} часов");
    }

    public static IRuleBuilderOptions<T, DateTimeOffset?> ValidExitTime<T>(
        this IRuleBuilder<T, DateTimeOffset?> ruleBuilder, Func<T, DateTimeOffset> entryTimeSelector)
    {
        return ruleBuilder
            .Must((_, exit) => exit == null || exit.Value != default).WithMessage("Время выхода некорректно")
            .Must((cmd, exit) => exit == null || exit.Value >= entryTimeSelector(cmd)).WithMessage("Время выхода не может быть раньше времени входа");
    }

    public static IRuleBuilderOptions<T, DateTimeOffset> ValidEntryTime<T>(
        this IRuleBuilder<T, DateTimeOffset> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty().WithMessage("Время входа обязательно")
            .Must(t => t != default).WithMessage("Время входа некорректно");
    }

    public static IRuleBuilderOptions<T, decimal?> ValidCalculatedCost<T>(
        this IRuleBuilder<T, decimal?> ruleBuilder)
    {
        return ruleBuilder
            .Must(cost => cost == null || cost >= 0).WithMessage("Стоимость не может быть отрицательной");
    }
}
