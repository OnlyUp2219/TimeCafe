namespace TimeCafe.Core.Models;

public partial class Theme
{
    public int ThemeId { get; set; }

    public string ThemeName { get; set; }

    public string TechnicalName { get; set; }

    public virtual ICollection<Tariff> Tariffs { get; set; } = new List<Tariff>();
}
