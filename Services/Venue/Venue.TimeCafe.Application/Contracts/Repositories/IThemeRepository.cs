namespace Venue.TimeCafe.Application.Contracts.Repositories;

public interface IThemeRepository
{
    Task<Theme?> GetByIdAsync(Guid themeId);
    Task<IEnumerable<Theme>> GetAllAsync();

    Task<Theme> CreateAsync(Theme theme);
    Task<Theme> UpdateAsync(Theme theme);
    Task<bool> DeleteAsync(Guid themeId);
}
