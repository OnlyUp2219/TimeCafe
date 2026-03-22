namespace BuildingBlocks.Validation;

public static class ProfileValidationExtensions
{
    public static IRuleBuilderOptions<T, DateOnly?> ValidBirthDate<T>(
        this IRuleBuilder<T, DateOnly?> ruleBuilder)
    {
        return ruleBuilder
            .Must(date => date == null || date <= DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Дата рождения не может быть в будущем");
    }
}
