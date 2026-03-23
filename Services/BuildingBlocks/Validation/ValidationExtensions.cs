namespace BuildingBlocks.Validation;

public static class ValidationExtensions
{
    public static IRuleBuilderOptions<T, string> ValidEntityId<T>(
        this IRuleBuilder<T, string> ruleBuilder, string message)
    {
        return ruleBuilder
            .NotEmpty().WithMessage(message)
            .NotNull().WithMessage(message)
            .Must(x => Guid.TryParse(x, out var guid) && guid != Guid.Empty)
            .WithMessage(message);
    }

    public static IRuleBuilderOptions<T, string?> ValidOptionalEntityId<T>(
        this IRuleBuilder<T, string?> ruleBuilder, string message)
    {
        return ruleBuilder
            .Must(x => string.IsNullOrWhiteSpace(x) || (Guid.TryParse(x, out var guid) && guid != Guid.Empty))
            .WithMessage(message);
    }

    public static IRuleBuilderOptions<T, Guid> ValidGuidEntityId<T>(
        this IRuleBuilder<T, Guid> ruleBuilder, string message)
    {
        return ruleBuilder
            .NotEqual(Guid.Empty).WithMessage(message);
    }

    public static IRuleBuilderOptions<T, Guid?> ValidOptionalGuidEntityId<T>(
        this IRuleBuilder<T, Guid?> ruleBuilder, string message)
    {
        return ruleBuilder
            .Must(x => !x.HasValue || x.Value != Guid.Empty)
            .WithMessage(message);
    }

    public static IRuleBuilderOptions<T, int> ValidPageNumber<T>(
        this IRuleBuilder<T, int> ruleBuilder)
    {
        return ruleBuilder
            .GreaterThan(0).WithMessage("Страница должна быть больше 0");
    }

    public static IRuleBuilderOptions<T, int> ValidPageSize<T>(
        this IRuleBuilder<T, int> ruleBuilder, int max = 100)
    {
        return ruleBuilder
            .GreaterThan(0).WithMessage("Размер страницы должен быть больше 0")
            .LessThanOrEqualTo(max).WithMessage($"Размер страницы не может быть больше {max}");
    }

    public static IRuleBuilderOptions<T, string> ValidEmail<T>(
        this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty().WithMessage("Email обязателен")
            .EmailAddress().WithMessage("Некорректный формат email");
    }

    public static IRuleBuilderOptions<T, string> ValidPhone<T>(
        this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty().WithMessage("Номер телефона не может быть пустым")
            .Matches(@"^\+\d{10,15}$").WithMessage("Неверный формат номера телефона");
    }

    public static IRuleBuilderOptions<T, string> ValidName<T>(
        this IRuleBuilder<T, string> ruleBuilder, string fieldName, int maxLength = 100)
    {
        return ruleBuilder
            .NotEmpty().WithMessage($"{fieldName} обязательно")
            .MaximumLength(maxLength).WithMessage($"{fieldName} не может превышать {maxLength} символов");
    }

    public static IRuleBuilderOptions<T, string?> ValidOptionalName<T>(
        this IRuleBuilder<T, string?> ruleBuilder, string fieldName, int maxLength = 100)
    {
        return ruleBuilder
            .MaximumLength(maxLength).WithMessage($"{fieldName} не может превышать {maxLength} символов");
    }

    public static IRuleBuilderOptions<T, string> ValidDescription<T>(
        this IRuleBuilder<T, string> ruleBuilder, int maxLength = 500)
    {
        return ruleBuilder
            .NotEmpty().WithMessage("Описание обязательно")
            .MaximumLength(maxLength).WithMessage($"Описание не может превышать {maxLength} символов");
    }

    public static IRuleBuilderOptions<T, string?> ValidOptionalDescription<T>(
        this IRuleBuilder<T, string?> ruleBuilder, int maxLength = 500)
    {
        return ruleBuilder
            .MaximumLength(maxLength).WithMessage($"Описание не может превышать {maxLength} символов");
    }

    public static IRuleBuilderOptions<T, string> ValidInfoText<T>(
        this IRuleBuilder<T, string> ruleBuilder, int maxLength = 2000)
    {
        return ruleBuilder
            .NotEmpty().WithMessage("Текст информации обязателен")
            .MaximumLength(maxLength).WithMessage($"Текст не может превышать {maxLength} символов");
    }

    public static IRuleBuilderOptions<T, decimal> ValidPrice<T>(
        this IRuleBuilder<T, decimal> ruleBuilder)
    {
        return ruleBuilder
            .GreaterThan(0).WithMessage("Цена должна быть больше 0");
    }

    public static IRuleBuilderOptions<T, string?> ValidUrl<T>(
        this IRuleBuilder<T, string?> ruleBuilder, string fieldName = "URL")
    {
        return ruleBuilder
            .Must(url => string.IsNullOrWhiteSpace(url) || Uri.IsWellFormedUriString(url, UriKind.Absolute))
            .WithMessage($"{fieldName} некорректен");
    }

    public static IRuleBuilderOptions<T, string?> ValidComment<T>(
        this IRuleBuilder<T, string?> ruleBuilder, int maxLength = 500)
    {
        return ruleBuilder
            .MaximumLength(maxLength).WithMessage($"Комментарий не может превышать {maxLength} символов");
    }

    public static IRuleBuilderOptions<T, string?> ValidCreatedBy<T>(
        this IRuleBuilder<T, string?> ruleBuilder, int maxLength = 450)
    {
        return ruleBuilder
            .MaximumLength(maxLength).WithMessage($"Поле автора не может превышать {maxLength} символов");
    }

    public static IRuleBuilderOptions<T, Guid> ValidGuidId<T>(
        this IRuleBuilder<T, Guid> ruleBuilder, string message)
    {
        return ruleBuilder
            .NotEmpty().WithMessage(message)
            .NotEqual(Guid.Empty).WithMessage(message);
    }
}
