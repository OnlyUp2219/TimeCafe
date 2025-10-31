namespace Auth.TimeCafe.Application.CQRS.Auth.Commands;

public record RefreshTokenCommand(string RefreshToken) : IRequest<Result<TokensDto>>;

public class RefreshTokenCommandHandler(IJwtService jwtService) : IRequestHandler<RefreshTokenCommand, Result<TokensDto>>
{
    private readonly IJwtService _jwtService = jwtService;

    public async Task<Result<TokensDto>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var tokens = await _jwtService.RefreshTokens(request.RefreshToken);
        
        if (tokens == null)
        {
            return Result<TokensDto>.Failure(new List<string> { "Недействительный или истекший refresh token" });
        }

        return Result<TokensDto>.Success(new TokensDto(tokens.AccessToken, tokens.RefreshToken));
    }
}
