namespace Auth.TimeCafe.Domain.Errors;

public sealed class UserNotFoundError : Error
{
    public UserNotFoundError(Guid userId)
        : base($"Пользователь '{userId}' не найден")
    {
        Metadata.Add("ErrorCode", "404");
        Metadata.Add("UserId", userId);
    }
}

public sealed class UserAlreadyInRoleError : Error
{
    public UserAlreadyInRoleError(Guid userId, string roleName)
        : base($"Пользователь '{userId}' уже имеет роль '{roleName}'")
    {
        Metadata.Add("ErrorCode", "400");
        Metadata.Add("UserId", userId);
        Metadata.Add("RoleName", roleName);
    }
}

public sealed class UserRoleNotAssignedError : Error
{
    public UserRoleNotAssignedError(Guid userId, string roleName)
        : base($"У пользователя '{userId}' отсутствует роль '{roleName}'")
    {
        Metadata.Add("ErrorCode", "400");
        Metadata.Add("UserId", userId);
        Metadata.Add("RoleName", roleName);
    }
}

public sealed class PhoneClearFailedError : Error
{
    public PhoneClearFailedError(Guid userId)
        : base($"Не удалось удалить номер телефона для пользователя '{userId}'")
    {
        Metadata.Add("ErrorCode", "500");
        Metadata.Add("Code", "PhoneClearFailed");
        Metadata.Add("UserId", userId);
    }
}

public sealed class PhoneVerificationSendFailedError : Error
{
    public PhoneVerificationSendFailedError(string phoneNumber)
        : base($"Ошибка при отправке SMS на номер '{phoneNumber}'")
    {
        Metadata.Add("ErrorCode", "500");
        Metadata.Add("Code", "SendFailed");
        Metadata.Add("PhoneNumber", phoneNumber);
    }
}

public sealed class PhoneSaveFailedError : Error
{
    public PhoneSaveFailedError(string phoneNumber)
        : base($"Не удалось сохранить номер телефона '{phoneNumber}'")
    {
        Metadata.Add("ErrorCode", "500");
        Metadata.Add("Code", "PhoneSaveFailed");
        Metadata.Add("PhoneNumber", phoneNumber);
    }
}

public sealed class PhoneVerificationTooManyAttemptsError : Error
{
    public PhoneVerificationTooManyAttemptsError(string phoneNumber)
        : base("Превышено количество попыток. Запросите новый код.")
    {
        Metadata.Add("ErrorCode", "429");
        Metadata.Add("Code", "TooManyAttempts");
        Metadata.Add("PhoneNumber", phoneNumber);
    }
}

public sealed class PhoneVerificationCaptchaRequiredError : Error
{
    public PhoneVerificationCaptchaRequiredError(string phoneNumber)
        : base("Пройдите проверку капчи")
    {
        Metadata.Add("ErrorCode", "400");
        Metadata.Add("Code", "CaptchaRequired");
        Metadata.Add("PhoneNumber", phoneNumber);
    }
}

public sealed class PhoneVerificationCaptchaInvalidError : Error
{
    public PhoneVerificationCaptchaInvalidError(string phoneNumber)
        : base("Неверная капча")
    {
        Metadata.Add("ErrorCode", "400");
        Metadata.Add("Code", "CaptchaInvalid");
        Metadata.Add("PhoneNumber", phoneNumber);
    }
}

public sealed class PhoneVerificationInvalidCodeError : Error
{
    public PhoneVerificationInvalidCodeError(string phoneNumber)
        : base("Неверный код подтверждения или истек срок действия")
    {
        Metadata.Add("ErrorCode", "400");
        Metadata.Add("Code", "InvalidCode");
        Metadata.Add("PhoneNumber", phoneNumber);
    }
}

public sealed class PhoneUserNotFoundError : Error
{
    public PhoneUserNotFoundError(Guid userId)
        : base($"Пользователь '{userId}' не найден")
    {
        Metadata.Add("ErrorCode", "401");
        Metadata.Add("Code", "UserNotFound");
        Metadata.Add("UserId", userId);
    }
}


