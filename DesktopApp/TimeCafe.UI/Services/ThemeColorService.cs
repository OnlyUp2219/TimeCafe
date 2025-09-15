namespace TimeCafe.UI.Services;

public class ThemeColorService : IThemeColorService
{
    private readonly Dictionary<string, string> _borderStyleKeys = new()
    {
        { "Red", "RedGradientBorderStyle" },
        { "Orange", "OrangeGradientBorderStyle" },
        { "Amber", "AmberGradientBorderStyle" },
        { "Yellow", "YellowGradientBorderStyle" },
        { "Lime", "LimeGradientBorderStyle" },
        { "Green", "GreenGradientBorderStyle" },
        { "Emerald", "EmeraldGradientBorderStyle" },
        { "Teal", "TealGradientBorderStyle" },
        { "Cyan", "CyanGradientBorderStyle" },
        { "Sky", "SkyGradientBorderStyle" },
        { "Blue", "BlueGradientBorderStyle" },
        { "Indigo", "IndigoGradientBorderStyle" },
        { "Violet", "VioletGradientBorderStyle" },
        { "Purple", "PurpleGradientBorderStyle" },
        { "Fuchsia", "FuchsiaGradientBorderStyle" },
        { "Pink", "PinkGradientBorderStyle" },
        { "Rose", "RoseGradientBorderStyle" },
        { "Slate", "SlateGradientBorderStyle" },
        { "Gray", "GrayGradientBorderStyle" },
        { "Zinc", "ZincGradientBorderStyle" },
        { "Neutral", "NeutralGradientBorderStyle" },
        { "Stone", "StoneGradientBorderStyle" }
    };

    private readonly Dictionary<string, string> _gradientBrushKeys = new()
    {
        { "Red", "RedGradientBrush" },
        { "Orange", "OrangeGradientBrush" },
        { "Amber", "AmberGradientBrush" },
        { "Yellow", "YellowGradientBrush" },
        { "Lime", "LimeGradientBrush" },
        { "Green", "GreenGradientBrush" },
        { "Emerald", "EmeraldGradientBrush" },
        { "Teal", "TealGradientBrush" },
        { "Cyan", "CyanGradientBrush" },
        { "Sky", "SkyGradientBrush" },
        { "Blue", "BlueGradientBrush" },
        { "Indigo", "IndigoGradientBrush" },
        { "Violet", "VioletGradientBrush" },
        { "Purple", "PurpleGradientBrush" },
        { "Fuchsia", "FuchsiaGradientBrush" },
        { "Pink", "PinkGradientBrush" },
        { "Rose", "RoseGradientBrush" },
        { "Slate", "SlateGradientBrush" },
        { "Gray", "GrayGradientBrush" },
        { "Zinc", "ZincGradientBrush" },
        { "Neutral", "NeutralGradientBrush" },
        { "Stone", "StoneGradientBrush" }
    };

    public Style GetThemeBorderStyle(string technicalName)
    {
        if (string.IsNullOrEmpty(technicalName) || !_borderStyleKeys.ContainsKey(technicalName))
        {
            return null;
        }

        var styleKey = _borderStyleKeys[technicalName];
        return Microsoft.UI.Xaml.Application.Current.Resources[styleKey] as Style;
    }

    public LinearGradientBrush GetThemeGradientBrush(string technicalName)
    {
        if (string.IsNullOrEmpty(technicalName) || !_gradientBrushKeys.ContainsKey(technicalName))
        {
            return null;
        }

        var brushKey = _gradientBrushKeys[technicalName];
        return Microsoft.UI.Xaml.Application.Current.Resources[brushKey] as LinearGradientBrush;
    }
}