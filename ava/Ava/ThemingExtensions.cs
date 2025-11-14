using Avalonia;
using Avalonia.Platform;
using Avalonia.Styling;

namespace Ava;

public static class ThemingExtensions
{
    private static Application? application;

    public static void RegisterSystemThemeAwareness(this Application app)
    {
        application = app;
        if (app.PlatformSettings is null) return;
        app.PlatformSettings.ColorValuesChanged -= OnColorValuesChanged;
        app.PlatformSettings.ColorValuesChanged += OnColorValuesChanged;
        OnColorValuesChanged(null, app.PlatformSettings?.GetColorValues());
    }

    public static void UnregisterSystemThemeAwareness(this Application app)
    {
        application = app;
        if (app.PlatformSettings is null) return;
        app.PlatformSettings.ColorValuesChanged -= OnColorValuesChanged;
    }

    private static void OnColorValuesChanged(object? _, PlatformColorValues? args)
    {
        if (args is not null && application is not null)
            application.RequestedThemeVariant = FindTheme(args);
    }

    private static ThemeVariant FindTheme(PlatformColorValues colorInfo) =>
        colorInfo.ContrastPreference is ColorContrastPreference.High
            ? AvaThemes.HighContrast
            : colorInfo.ThemeVariant switch
            {
                PlatformThemeVariant.Light => AvaThemes.Light,
                PlatformThemeVariant.Dark => AvaThemes.Dark,
                _ => AvaThemes.Default
            };
}