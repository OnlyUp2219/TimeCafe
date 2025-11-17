namespace Auth.TimeCafe.Application.CQRS.Auth.Commands;

public record class RefreshTokenCommand(string RefreshToken) : IRequest<RefreshTokenResult>;

public record class RefreshTokenResult(
    bool Success,
    string? Code = null,
    string? Message = null,
    int? StatusCode = null,
    List<ErrorItem>? Errors = null,
    string? AccessToken = null,
    string? RefreshToken = null) : ICqrsResultV2
{
    public static RefreshTokenResult InvalidToken() =>
        new(false, Code: "InvalidToken", Message: "Токен недействителен или уже использован", StatusCode: 401);

    public static RefreshTokenResult TokenSuccess(string accessToken, string refreshToken) =>
        new(true, Message: "Токен обновлён",
            AccessToken: accessToken, RefreshToken: refreshToken);
}

public class RefreshTokenCommandHandler(IJwtService jwtService) : IRequestHandler<RefreshTokenCommand, RefreshTokenResult>
{
    private readonly IJwtService _jwtService = jwtService;

    public async Task<RefreshTokenResult> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var tokens = await _jwtService.RefreshTokens(request.RefreshToken);
        if (tokens == null)
            return RefreshTokenResult.InvalidToken();


        return RefreshTokenResult.TokenSuccess(tokens.AccessToken, tokens.RefreshToken);
    }
}
