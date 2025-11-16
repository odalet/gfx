using Avalonia.Platform;
using Avalonia.Styling;

namespace Ava.Theming;

public static class ThemingUtils
{
    public static ThemeVariant Light => ThemeVariant.Light;
    public static ThemeVariant Dark => ThemeVariant.Dark;
    public static ThemeVariant Default => ThemeVariant.Default;
    public static ThemeVariant HighContrast { get; } = new(nameof(HighContrast), ThemeVariant.Dark);

    public static ThemeVariant FindThemeVariant(PlatformColorValues colorInfo) =>
        colorInfo.ContrastPreference is ColorContrastPreference.High
            ? HighContrast : colorInfo.ThemeVariant switch
            {
                PlatformThemeVariant.Light => ThemeVariant.Light,
                PlatformThemeVariant.Dark => ThemeVariant.Dark,
                _ => ThemeVariant.Default
            };
}
