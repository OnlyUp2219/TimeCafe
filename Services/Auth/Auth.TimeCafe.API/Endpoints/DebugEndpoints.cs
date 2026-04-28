using Carter;
using BuildingBlocks.Extensions;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Auth.TimeCafe.API.Endpoints;

public class DebugEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/debug").WithTags("Debug");

        group.MapGet("/error/404-single", () => 
        {
            var result = Result.Fail(new Error("Пользователь не найден")
                .WithMetadata("ErrorCode", 404)
                .WithMetadata("Code", "UserNotFound"));
            return result.ToHttpResult(() => Results.Ok());
        });

        group.MapGet("/error/422-multiple", () => 
        {
            var result = Result.Fail(new Error("Ошибка валидации профиля")
                .WithMetadata("ErrorCode", 422)
                .WithMetadata("Code", "ValidationError"));
            
            result.WithError(new Error("Имя обязательно для заполнения").WithMetadata("Code", "FirstName"));
            result.WithError(new Error("Фамилия обязательна для заполнения").WithMetadata("Code", "LastName"));
            result.WithError(new Error("Возраст должен быть больше 18").WithMetadata("Code", "Age"));
            
            return result.ToHttpResult(() => Results.Ok());
        });

        group.MapGet("/error/500-single", () => 
        {
            var result = Result.Fail(new Error("Критическая ошибка базы данных")
                .WithMetadata("ErrorCode", 500)
                .WithMetadata("Code", "DbTimeout"));
            return result.ToHttpResult(() => Results.Ok());
        });

        group.MapGet("/error/500-multiple", () => 
        {
            var result = Result.Fail(new Error("Множественные системные сбои")
                .WithMetadata("ErrorCode", 500)
                .WithMetadata("Code", "SystemFailure"));
            
            result.WithError(new Error("Redis недоступен (Connection refused)").WithMetadata("Code", "Redis"));
            result.WithError(new Error("RabbitMQ очередь переполнена").WithMetadata("Code", "RabbitMQ"));
            
            return result.ToHttpResult(() => Results.Ok());
        });

        group.MapGet("/error/exception", () => {
            throw new Exception("Необработанное исключение сервера (Unhandled Exception)!");
        });

        group.MapGet("/success", () => 
        {
            var result = Result.Ok(new { id = Guid.NewGuid(), status = "Processed" });
            return result.ToHttpResult(data => Results.Ok(new { message = "Данные успешно сохранены", data }));
        });

        group.MapGet("/info", () => 
        {
            // Эмуляция информационного ответа через метаданные успеха или кастомный объект
            return Results.Ok(new { message = "Ваш пробный период истекает. Пожалуйста, обновите тариф.", statusCode = 200, code = "SubscriptionInfo" });
        });
            
        group.MapGet("/legacy-result", () => 
        {
            var result = new LegacyResult
            {
                Success = false,
                StatusCode = 400,
                Code = "Legacy_InvalidOperation",
                Message = "Ошибка, возвращенная через ICqrsResult.ToHttpResult",
                Errors = new List<ErrorItem>
                {
                    new("Field1", "Описание ошибки 1"),
                    new("Field2", "Описание ошибки 2")
                }
            };
            
            return result.ToHttpResult(r => Results.Ok(r));
        });
    }

    private class LegacyResult : ICqrsResult
    {
        public bool Success { get; set; }
        public int? StatusCode { get; set; }
        public string? Code { get; set; }
        public string? Message { get; set; }
        public List<ErrorItem>? Errors { get; set; }
    }
}
