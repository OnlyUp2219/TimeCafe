namespace Auth.TimeCafe.Application.CQRS.Auth.Commands;

public record class LogoutCommand(string RefreshToken) : IRequest<LogoutResult>;

public record class LogoutResult(bool Success, string? Message = null, List<string>? Errors = null, ETypeError? TypeError = null, bool? Revoked = null) : ICqrsResult;

public class LogoutCommandHandler(IJwtService jwtService) : IRequestHandler<LogoutCommand, LogoutResult>
{
    private readonly IJwtService _jwtService = jwtService;

    public async Task<LogoutResult> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
            return new LogoutResult(
                Success: false,
                Message: "Refresh token обязателен",
                Errors: ["TokenNotFound"],
                Revoked: false);

        var revoked = await _jwtService.RevokeRefreshTokenAsync(request.RefreshToken, cancellationToken);

        return new LogoutResult(
            Success: true,
            Message: revoked ? "Выход осуществлен" : "Токен не найден",
            Revoked: revoked);
    }
}