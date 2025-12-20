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


    public static Theme Create(Guid? themeId, string name, string? emoji = null, string? colors = null)
    {
        return new Theme
        {
            ThemeId = themeId ?? Guid.NewGuid(),
            Name = name,
            Emoji = emoji,
            Colors = colors
        };
    }

    public static Theme Update(Theme existingTheme, string? name = null, string? emoji = null, string? colors = null)
    {
        return new Theme(existingTheme.ThemeId)
        {
            Name = name ?? existingTheme.Name,
            Emoji = emoji ?? existingTheme.Emoji,
            Colors = colors ?? existingTheme.Colors
        };
    }
}
