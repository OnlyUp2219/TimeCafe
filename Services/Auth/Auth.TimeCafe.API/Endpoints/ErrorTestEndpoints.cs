using BuildingBlocks.Common;
using BuildingBlocks.Extensions;

namespace Auth.TimeCafe.API.Endpoints;

public class ErrorTestEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/error-test")
            .WithTags("Error Testing");

        group.MapGet("/validation", () =>
        {
            var errors = new List<ErrorDetail>
            {
                ErrorDetail.Validation("email", "Email должен быть валидным", "Validation.Email.Invalid"),
                ErrorDetail.Validation("password", "Пароль должен содержать минимум 6 символов", "Validation.Password.TooShort"),
                ErrorDetail.Validation("username", "Имя пользователя уже занято", "Validation.Username.Taken")
            };
            return ErrorResultHelper.ToHttpResult(errors);
        })
        .WithName("TestValidationError")
        .WithSummary("Тест ошибки валидации");

        group.MapGet("/not-found", () =>
        {
            return ErrorResultHelper.ToHttpResult(ErrorDetail.NotFound("User", "123"));
        })
        .WithName("TestNotFoundError")
        .WithSummary("Тест ошибки NotFound");

        group.MapGet("/unauthorized", () =>
        {
            return ErrorResultHelper.ToHttpResult(ErrorDetail.Unauthorized("Необходима авторизация"));
        })
        .WithName("TestUnauthorizedError")
        .WithSummary("Тест ошибки Unauthorized");

        group.MapGet("/forbidden", () =>
        {
            return ErrorResultHelper.ToHttpResult(ErrorDetail.Forbidden("У вас недостаточно прав для выполнения этого действия"));
        })
        .WithName("TestForbiddenError")
        .WithSummary("Тест ошибки Forbidden");

        group.MapGet("/conflict", () =>
        {
            return ErrorResultHelper.ToHttpResult(ErrorDetail.Conflict("User.AlreadyExists", "Пользователь с таким email уже существует"));
        })
        .WithName("TestConflictError")
        .WithSummary("Тест ошибки Conflict");

        group.MapGet("/rate-limit", () =>
        {
            var error = ErrorDetail.RateLimit("Слишком много запросов. Попробуйте позже.", 60);
            return ErrorResultHelper.ToHttpResult(error);
        })
        .WithName("TestRateLimitError")
        .WithSummary("Тест ошибки RateLimit");

        group.MapGet("/business-logic", () =>
        {
            return ErrorResultHelper.ToHttpResult(ErrorDetail.BusinessLogic("Email.NotConfirmed", "Подтвердите email перед входом"));
        })
        .WithName("TestBusinessLogicError")
        .WithSummary("Тест ошибки BusinessLogic");

        group.MapGet("/critical", () =>
        {
            return ErrorResultHelper.ToHttpResult(ErrorDetail.Critical("Внутренняя ошибка сервера. Попробуйте позже."));
        })
        .WithName("TestCriticalError")
        .WithSummary("Тест ошибки Critical");

        group.MapGet("/multiple-validation", () =>
        {
            var errors = new List<ErrorDetail>
            {
                ErrorDetail.Validation("firstName", "Имя обязательно для заполнения"),
                ErrorDetail.Validation("lastName", "Фамилия обязательна для заполнения"),
                ErrorDetail.Validation("age", "Возраст должен быть от 18 до 100"),
                ErrorDetail.Validation("phoneNumber", "Неверный формат телефона")
            };
            return ErrorResultHelper.ToHttpResult(errors);
        })
        .WithName("TestMultipleValidationErrors")
        .WithSummary("Тест множественных ошибок валидации");

        group.MapGet("/success", () =>
        {
            return Results.Ok(new { message = "Успешный запрос без ошибок", timestamp = DateTime.UtcNow });
        })
        .WithName("TestSuccess")
        .WithSummary("Тест успешного ответа");
    }
}
