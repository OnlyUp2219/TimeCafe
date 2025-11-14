namespace Auth.TimeCafe.Application.CQRS.Auth.Commands;

public record class LogoutCommand(string RefreshToken) : IRequest<LogoutResult>;

public record class LogoutResult(
    bool Success,
    string? Code = null,
    string? Message = null,
    int? StatusCode = null,
    List<ErrorItem>? Errors = null,
    bool? Revoked = null) : ICqrsResultV2
{
    public static LogoutResult TokenNotFound() =>
        new(false, Code: "TokenNotFound", Message: "Токен не найден", StatusCode: 400,
            Revoked: false);

    public static LogoutResult LoggedOut(bool revoked) =>
        new(true, Code: revoked ? "Выход осуществлен" : "Токен не найден", 
            Revoked: revoked);
}

public class LogoutCommandHandler(IJwtService jwtService) : IRequestHandler<LogoutCommand, LogoutResult>
{
    private readonly IJwtService _jwtService = jwtService;

    public async Task<LogoutResult> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
            return LogoutResult.TokenNotFound();

        var revoked = await _jwtService.RevokeRefreshTokenAsync(request.RefreshToken, cancellationToken);

        return LogoutResult.LoggedOut(revoked);
    }
}