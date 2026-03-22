namespace Venue.TimeCafe.Application.Contracts.Repositories;

public interface IThemeRepository
{
    Task<Theme?> GetByIdAsync(Guid themeId, CancellationToken ct = default);
    Task<IEnumerable<Theme>> GetAllAsync(CancellationToken ct = default);

    Task<Theme> CreateAsync(Theme theme, CancellationToken ct = default);
    Task<Theme> UpdateAsync(Theme theme, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid themeId, CancellationToken ct = default);
}
