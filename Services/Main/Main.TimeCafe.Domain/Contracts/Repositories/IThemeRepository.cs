namespace Main.TimeCafe.Domain.Contracts.Repositories;

public interface IThemeRepository
{
    Task<IEnumerable<Theme>> GetThemesAsync();
}