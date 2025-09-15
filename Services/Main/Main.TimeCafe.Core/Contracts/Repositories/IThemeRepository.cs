using TimeCafe.Core.Models;

namespace TimeCafe.Core.Contracts.Repositories;

public interface IThemeRepository
{
    Task<IEnumerable<Theme>> GetThemesAsync();
}