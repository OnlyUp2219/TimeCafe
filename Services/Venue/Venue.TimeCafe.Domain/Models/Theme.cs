namespace Venue.TimeCafe.Domain.Models;

public class Theme
{
    public Theme(Guid themeId)
    {
        ThemeId = themeId;
    }

    public Theme()
    {
        ThemeId = Guid.NewGuid();
    }

    public Guid ThemeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Emoji { get; set; }
    public string? Colors { get; set; }

    public virtual ICollection<Tariff> Tariffs { get; set; } = [];
}
