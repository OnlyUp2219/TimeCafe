namespace TimeCafe.UI.Contracts.Services;

public interface IThemeColorService
{
    Style GetThemeBorderStyle(string technicalName);
    LinearGradientBrush GetThemeGradientBrush(string technicalName);
}