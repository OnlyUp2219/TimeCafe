namespace BuildingBlocks.Extensions;

public static class ValidatorExtensions
{
    public static IRuleBuilderOptions<T, string> BeValidBase64<T>(
        this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .Must(str =>
            {
                if (string.IsNullOrWhiteSpace(str)) return false;
                try
                {
                    Convert.FromBase64String(str.Trim());
                    return true;
                }
                catch
                {
                    return false;
                }
            })
            .WithMessage("{PropertyName} должен быть валидной Base64-строкой");
    }
}