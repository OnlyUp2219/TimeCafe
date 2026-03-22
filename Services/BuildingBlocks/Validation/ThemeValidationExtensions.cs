namespace BuildingBlocks.Validation;

public static class ThemeValidationExtensions
{
    public static IRuleBuilderOptions<T, string?> ValidEmoji<T>(
        this IRuleBuilder<T, string?> ruleBuilder, int maxLength = 10)
    {
        return ruleBuilder
            .MaximumLength(maxLength).WithMessage($"Эмодзи не может превышать {maxLength} символов");
    }
}
