namespace Venue.TimeCafe.Domain.Models;

public class Theme
{
    public int ThemeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Emoji { get; set; }
    public string? Colors { get; set; }

    public virtual ICollection<Tariff> Tariffs { get; set; } = new List<Tariff>();
}
