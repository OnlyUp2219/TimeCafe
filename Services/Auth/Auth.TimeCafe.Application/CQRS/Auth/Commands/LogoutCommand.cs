namespace Auth.TimeCafe.Application.CQRS.Auth.Commands;

public record class LogoutCommand(string RefreshToken) : IRequest<LogoutResult>;

public class LogoutCommandValidator : AbstractValidator<LogoutCommand>
{
    public LogoutCommandValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token обязателен");
    }
}

public record class LogoutResult(
    bool Success,
    bool Revoked,
    string? Code = null,
    string? Message = null,
    int? StatusCode = null,
    List<ErrorItem>? Errors = null) : ICqrsResultV2
{
    public static LogoutResult NoToken() =>
        new(true, false, Message: "Refresh token отсутствует");

    public static LogoutResult Completed(bool revoked) =>
        new(true, revoked,
            Code: revoked ? "LogoutCompleted" : "LogoutSkipped",
            Message: revoked ? "Refresh token отозван" : "Refresh token не найден");
}

public class LogoutCommandHandler(IJwtService jwtService) : IRequestHandler<LogoutCommand, LogoutResult>
{
    private readonly IJwtService _jwtService = jwtService;

    public async Task<LogoutResult> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
            return LogoutResult.NoToken();

        var revoked = await _jwtService.RevokeRefreshTokenAsync(request.RefreshToken, cancellationToken);
        return LogoutResult.Completed(revoked);
    }
}