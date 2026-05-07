namespace Venue.TimeCafe.Application.Contracts.Repositories;

public interface IThemeRepository
{
    Task<Theme?> GetByIdAsync(Guid themeId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Theme>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Theme>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default);

    Task<Theme> CreateAsync(Theme theme, CancellationToken cancellationToken = default);
    Task<Theme> UpdateAsync(Theme theme, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid themeId, CancellationToken cancellationToken = default);
}

