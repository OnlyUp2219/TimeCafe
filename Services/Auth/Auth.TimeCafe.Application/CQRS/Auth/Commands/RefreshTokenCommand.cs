namespace Auth.TimeCafe.Application.CQRS.Auth.Commands;

public record class RefreshTokenCommand(string RefreshToken) : IRequest<RefreshTokenResult>;
public record class RefreshTokenResult(bool Success, string? Message = null, List<string>? Errors = null, ETypeError? TypeError = null, string? AccessToken = null, string? RefreshToken = null) : ICqrsResult;

public class RefreshTokenCommandhandler(IJwtService jwtService) : IRequestHandler<RefreshTokenCommand, RefreshTokenResult>
{
    private readonly IJwtService _jwtService = jwtService;

    public async Task<RefreshTokenResult> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var tokens = await _jwtService.RefreshTokens(request.RefreshToken);
        if (tokens == null)
            return new RefreshTokenResult(
                false,
                Message: "Токен истек или невалиден",
                TypeError: ETypeError.Unauthorized);


        return new RefreshTokenResult(
            true,
            Message: "Токен обновлён",
            AccessToken: tokens.AccessToken,
            RefreshToken: tokens.RefreshToken);
    }
}
