namespace Auth.TimeCafe.Application.CQRS.Auth.Commands;

public record LogoutCommand(string RefreshToken, string? UserId = null) : IRequest<Result<bool>>;

public class LogoutCommandHandler(IRefreshTokenRepository refreshTokenRepository) : IRequestHandler<LogoutCommand, Result<bool>>
{
    private readonly IRefreshTokenRepository _refreshTokenRepository = refreshTokenRepository;

    public async Task<Result<bool>> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return Result<bool>.ValidationError("refreshToken", "Refresh token обязателен");
        }

        var tokenEntity = await _refreshTokenRepository.GetActiveTokenAsync(request.RefreshToken, cancellationToken);

        if (tokenEntity == null)
        {
            return Result<bool>.Success(false);
        }

        if (request.UserId != null && tokenEntity.UserId != request.UserId)
        {
            return Result<bool>.Unauthorized("Недостаточно прав для отзыва данного токена");
        }

        tokenEntity.IsRevoked = true;
        await _refreshTokenRepository.UpdateAsync(tokenEntity, cancellationToken);
        await _refreshTokenRepository.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
