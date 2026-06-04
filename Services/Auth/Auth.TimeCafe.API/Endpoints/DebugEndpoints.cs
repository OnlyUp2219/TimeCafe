namespace Auth.TimeCafe.API.Endpoints;

public class DebugEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/debug").WithTags("Debug");

        group.MapGet("/error/404-single", () => 
        {
            var result = FluentResults.Result.Fail(new FluentResults.Error("Пользователь не найден")
                .WithMetadata("ErrorCode", 404)
                .WithMetadata("Code", "UserNotFound"));
            return result.ToHttpResult(() => Results.Ok());
        })
        .WithName("DebugError404Single")
        .WithSummary("Debug 404 Single Error")
        .WithDescription("Returns a single 404 error for debugging.");

        group.MapGet("/error/422-multiple", () => 
        {
            var result = FluentResults.Result.Fail(new FluentResults.Error("Ошибка валидации профиля")
                .WithMetadata("ErrorCode", 422)
                .WithMetadata("Code", "ValidationError"));
            
            result.WithError(new FluentResults.Error("Имя обязательно для заполнения").WithMetadata("Code", "FirstName"));
            result.WithError(new FluentResults.Error("Фамилия обязательна для заполнения").WithMetadata("Code", "LastName"));
            result.WithError(new FluentResults.Error("Возраст должен быть больше 18").WithMetadata("Code", "Age"));
            
            return result.ToHttpResult(() => Results.Ok());
        })
        .WithName("DebugError422Multiple")
        .WithSummary("Debug 422 Multiple Errors")
        .WithDescription("Returns multiple 422 validation errors for debugging.");

        group.MapGet("/error/500-single", () => 
        {
            var result = FluentResults.Result.Fail(new FluentResults.Error("Критическая ошибка базы данных")
                .WithMetadata("ErrorCode", 500)
                .WithMetadata("Code", "DbTimeout"));
            return result.ToHttpResult(() => Results.Ok());
        })
        .WithName("DebugError500Single")
        .WithSummary("Debug 500 Single Error")
        .WithDescription("Returns a single 500 error for debugging.");

        group.MapGet("/error/500-multiple", () => 
        {
            var result = FluentResults.Result.Fail(new FluentResults.Error("Множественные системные сбои")
                .WithMetadata("ErrorCode", 500)
                .WithMetadata("Code", "SystemFailure"));
            
            result.WithError(new FluentResults.Error("Redis недоступен (Connection refused)").WithMetadata("Code", "Redis"));
            result.WithError(new FluentResults.Error("RabbitMQ очередь переполнена").WithMetadata("Code", "RabbitMQ"));
            
            return result.ToHttpResult(() => Results.Ok());
        })
        .WithName("DebugError500Multiple")
        .WithSummary("Debug 500 Multiple Errors")
        .WithDescription("Returns multiple 500 errors for debugging.");

        group.MapGet("/error/exception", () => {
            throw new System.Exception("Необработанное исключение сервера (Unhandled Exception)!");
        })
        .WithName("DebugException")
        .WithSummary("Debug Exception")
        .WithDescription("Throws an unhandled exception for debugging.");

        group.MapGet("/success", () => 
        {
            var result = FluentResults.Result.Ok(new { id = System.Guid.NewGuid(), status = "Processed" });
            return result.ToHttpResult(data => Results.Ok(new { message = "Данные успешно сохранены", data }));
        })
        .WithName("DebugSuccess")
        .WithSummary("Debug Success")
        .WithDescription("Returns a successful result for debugging.");

        group.MapGet("/info", () =>
            Results.Ok(new { message = "Ваш пробный период истекает. Пожалуйста, обновите тариф.", statusCode = 200, code = "SubscriptionInfo" }))
        .WithName("DebugInfo")
        .WithSummary("Debug Info")
        .WithDescription("Returns info message for debugging.");
            
        group.MapGet("/legacy-result", () => 
        {
            var result = new LegacyResult
            {
                Success = false,
                StatusCode = 400,
                Code = "Legacy_InvalidOperation",
                Message = "Ошибка, возвращенная через ICqrsResult.ToHttpResult",
                Errors =
                [
                    new("Field1", "Описание ошибки 1"),
                    new("Field2", "Описание ошибки 2")
                ]
            };
            
            return result.ToHttpResult(r => Results.Ok(r));
        })
        .WithName("DebugLegacyResult")
        .WithSummary("Debug Legacy Result")
        .WithDescription("Returns a legacy result for debugging.");
    }

    private class LegacyResult : ICqrsResult
    {
        public bool Success { get; set; }
        public int? StatusCode { get; set; }
        public string? Code { get; set; }
        public string? Message { get; set; }
        public System.Collections.Generic.List<ErrorItem>? Errors { get; set; }
    }
}
