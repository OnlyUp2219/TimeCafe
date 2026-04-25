namespace BuildingBlocks.Validation;

public static class PaymentValidationExtensions
{
    public static IRuleBuilderOptions<T, decimal> ValidPaymentAmount<T>(
        this IRuleBuilder<T, decimal> ruleBuilder, decimal minAmount = 50)
    {
        return ruleBuilder
            .GreaterThan(0).WithMessage("Сумма должна быть больше нуля")
            .GreaterThanOrEqualTo(minAmount).WithMessage($"Минимальная сумма платежа {minAmount} ₽");
    }

    public static IRuleBuilderOptions<T, string?> ValidUrlWithPlaceholder<T>(
        this IRuleBuilder<T, string?> ruleBuilder, string fieldName = "URL")
    {
        return ruleBuilder
            .Must(url =>
            {
                if (string.IsNullOrWhiteSpace(url))
                    return true;
                var urlToValidate = url.Replace("{CHECKOUT_SESSION_ID}", "placeholder");
                return Uri.IsWellFormedUriString(urlToValidate, UriKind.Absolute);
            })
            .WithMessage($"{fieldName} некорректен");
    }
}
