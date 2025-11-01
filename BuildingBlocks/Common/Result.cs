namespace BuildingBlocks.Common;

public class Result<T>
{
    public bool IsSuccess { get; }
    public T? Data { get; }
    public List<ErrorDetail> Errors { get; }

    private Result(bool isSuccess, T? data, List<ErrorDetail> errors)
    {
        IsSuccess = isSuccess;
        Data = data;
        Errors = errors ?? new List<ErrorDetail>();
    }

    public static Result<T> Success(T data) => new(true, data, new List<ErrorDetail>());

    public static Result<T> Failure(List<ErrorDetail> errors) => new(false, default, errors);

    public static Result<T> Failure(ErrorDetail error) => new(false, default, new List<ErrorDetail> { error });

    [Obsolete("Use Failure(ErrorDetail) instead")]
    public static Result<T> Failure(string error) =>
        new(false, default, new List<ErrorDetail> { ErrorDetail.Critical(error) });

    [Obsolete("Use Failure(List<ErrorDetail>) instead")]
    public static Result<T> Failure(List<string> errors) =>
        new(false, default, errors.Select(e => ErrorDetail.Critical(e)).ToList());

    public static Result<T> ValidationError(string field, string message, string? code = null) =>
        Failure(ErrorDetail.Validation(field, message, code));

    public static Result<T> ValidationErrors(List<ErrorDetail> validationErrors) =>
        Failure(validationErrors);

    public static Result<T> NotFound(string resourceName, string? resourceId = null) =>
        Failure(ErrorDetail.NotFound(resourceName, resourceId));

    public static Result<T> Unauthorized(string message = "Необходима авторизация") =>
        Failure(ErrorDetail.Unauthorized(message));

    public static Result<T> Forbidden(string message = "Недостаточно прав") =>
        Failure(ErrorDetail.Forbidden(message));

    public static Result<T> Conflict(string code, string message) =>
        Failure(ErrorDetail.Conflict(code, message));

    public static Result<T> BusinessLogic(string code, string message) =>
        Failure(ErrorDetail.BusinessLogic(code, message));

    public static Result<T> Critical(string message, string? code = null) =>
        Failure(ErrorDetail.Critical(message, code));

    public static Result<T> RateLimit(string message, int? remainingSeconds = null) =>
        Failure(ErrorDetail.RateLimit(message, remainingSeconds));
}
