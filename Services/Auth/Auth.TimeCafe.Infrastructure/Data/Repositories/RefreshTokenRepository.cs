namespace Auth.TimeCafe.Infrastructure.Data.Repositories;

public class RefreshTokenRepository(ApplicationDbContext context) : IRefreshTokenRepository
{
    private readonly ApplicationDbContext _context = context;

    public async Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return await _context.RefreshTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Token == token, cancellationToken);
    }

    public async Task<RefreshToken?> GetActiveTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return await _context.RefreshTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Token == token && !t.IsRevoked && t.Expires > DateTime.UtcNow, cancellationToken);
    }

    public async Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
    {
        await _context.RefreshTokens.AddAsync(refreshToken, cancellationToken);
    }

    public Task UpdateAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
    {
        _context.RefreshTokens.Update(refreshToken);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
