namespace BuildingBlocks.Common;

public class ErrorDetail
{
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Field { get; set; }
    public ErrorType Type { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
    public ErrorDetail() { }
    public ErrorDetail(ErrorType type, string code, string message, string? field = null)
    {
        Type = type;
        Code = code;
        Message = message;
        Field = field;
    }

    public static ErrorDetail Validation(string field, string message, string? code = null)
    {
        return new ErrorDetail(
            ErrorType.Validation,
            code ?? $"Validation.{field}",
            message,
            field
        );
    }

    public static ErrorDetail NotFound(string resourceName, string? resourceId = null)
    {
        var message = resourceId != null
            ? $"{resourceName} с ID '{resourceId}' не найден"
            : $"{resourceName} не найден";

        return new ErrorDetail(
            ErrorType.NotFound,
            $"{resourceName}.NotFound",
            message
        );
    }

    public static ErrorDetail BusinessLogic(string code, string message)
    {
        return new ErrorDetail(ErrorType.BusinessLogic, code, message);
    }

    public static ErrorDetail Critical(string message, string? code = null)
    {
        return new ErrorDetail(
            ErrorType.Critical,
            code ?? "Server.Critical",
            message
        );
    }
    public static ErrorDetail Unauthorized(string message = "Необходима авторизация")
    {
        return new ErrorDetail(ErrorType.Unauthorized, "Auth.Unauthorized", message);
    }

    public static ErrorDetail Forbidden(string message = "Недостаточно прав")
    {
        return new ErrorDetail(ErrorType.Forbidden, "Auth.Forbidden", message);
    }

    public static ErrorDetail Conflict(string code, string message)
    {
        return new ErrorDetail(ErrorType.Conflict, code, message);
    }

    public static ErrorDetail RateLimit(string message, int? remainingSeconds = null)
    {
        var error = new ErrorDetail(ErrorType.RateLimit, "RateLimit.Exceeded", message);

        if (remainingSeconds.HasValue)
        {
            error.Metadata = new Dictionary<string, object>
            {
                ["remainingSeconds"] = remainingSeconds.Value
            };
        }

        return error;
    }
}
