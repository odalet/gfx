using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Styling;

namespace Ava.Themes.Palettes;

// Copied from Avalonia's Fluent Theme
internal sealed class SystemAccentColors : ResourceProvider
{
    public const string AccentKey = "SystemAccentColor";
    public const string AccentDark1Key = "SystemAccentColorDark1";
    public const string AccentDark2Key = "SystemAccentColorDark2";
    public const string AccentDark3Key = "SystemAccentColorDark3";
    public const string AccentLight1Key = "SystemAccentColorLight1";
    public const string AccentLight2Key = "SystemAccentColorLight2";
    public const string AccentLight3Key = "SystemAccentColorLight3";

    private static readonly Color defaultSystemAccentColor = Color.FromRgb(0, 120, 215);
    private bool invalidateColors = true;
    private Color systemAccentColor;
    private Color systemAccentColorDark1, systemAccentColorDark2, systemAccentColorDark3;
    private Color systemAccentColorLight1, systemAccentColorLight2, systemAccentColorLight3;

    public override bool HasResources => true;

    public override bool TryGetResource(object key, ThemeVariant? theme, out object? value)
    {
        value = null;
        if (key is not string str)
            return false;

        if (str.Equals(AccentKey, StringComparison.InvariantCulture))
        {
            EnsureColors();
            value = systemAccentColor;
            return true;
        }

        if (str.Equals(AccentDark1Key, StringComparison.InvariantCulture))
        {
            EnsureColors();
            value = systemAccentColorDark1;
            return true;
        }

        if (str.Equals(AccentDark2Key, StringComparison.InvariantCulture))
        {
            EnsureColors();
            value = systemAccentColorDark2;
            return true;
        }

        if (str.Equals(AccentDark3Key, StringComparison.InvariantCulture))
        {
            EnsureColors();
            value = systemAccentColorDark3;
            return true;
        }

        if (str.Equals(AccentLight1Key, StringComparison.InvariantCulture))
        {
            EnsureColors();
            value = systemAccentColorLight1;
            return true;
        }

        if (str.Equals(AccentLight2Key, StringComparison.InvariantCulture))
        {
            EnsureColors();
            value = systemAccentColorLight2;
            return true;
        }

        if (str.Equals(AccentLight3Key, StringComparison.InvariantCulture))
        {
            EnsureColors();
            value = systemAccentColorLight3;
            return true;
        }

        return false;
    }

    public static (Color d1, Color d2, Color d3, Color l1, Color l2, Color l3) CalculateAccentShades(Color accentColor)
    {
        // dark1step = (hslAccent.L - SystemAccentColorDark1.L) * 255
        const double dark1step = 28.5 / 255.0;
        const double dark2step = 49.0 / 255.0;
        const double dark3step = 74.5 / 255.0;
        // light1step = (SystemAccentColorLight1.L - hslAccent.L) * 255
        const double light1step = 39.0 / 255.0;
        const double light2step = 70.0 / 255.0;
        const double light3step = 103.0 / 255.0;

        var accent = accentColor.ToHsl();
        return (
            // Darker shades
            new HslColor(accent.A, accent.H, accent.S, accent.L - dark1step).ToRgb(),
            new HslColor(accent.A, accent.H, accent.S, accent.L - dark2step).ToRgb(),
            new HslColor(accent.A, accent.H, accent.S, accent.L - dark3step).ToRgb(),

            // Lighter shades
            new HslColor(accent.A, accent.H, accent.S, accent.L + light1step).ToRgb(),
            new HslColor(accent.A, accent.H, accent.S, accent.L + light2step).ToRgb(),
            new HslColor(accent.A, accent.H, accent.S, accent.L + light3step).ToRgb()
        );
    }

    protected override void OnAddOwner(IResourceHost owner)
    {
        if (GetFromOwner(owner) is { } platformSettings)
            platformSettings.ColorValuesChanged += OnColorValuesChanged;
        invalidateColors = true;
    }

    protected override void OnRemoveOwner(IResourceHost owner)
    {
        if (GetFromOwner(owner) is { } platformSettings)
            platformSettings.ColorValuesChanged -= OnColorValuesChanged;
        invalidateColors = true;
    }

    private void EnsureColors()
    {
        if (!invalidateColors) return;

        invalidateColors = false;
        var platformSettings = GetFromOwner(Owner);

        systemAccentColor = platformSettings?.GetColorValues().AccentColor1 ?? defaultSystemAccentColor;
        (systemAccentColorDark1, systemAccentColorDark2, systemAccentColorDark3, systemAccentColorLight1, systemAccentColorLight2, systemAccentColorLight3) = 
            CalculateAccentShades(systemAccentColor);
    }

    private static IPlatformSettings? GetFromOwner(IResourceHost? owner) => owner switch
    {
        Application app => app.PlatformSettings,
        Visual visual => TopLevel.GetTopLevel(visual)?.PlatformSettings,
        _ => null
    };

    private void OnColorValuesChanged(object? sender, PlatformColorValues e)
    {
        invalidateColors = true;
        Owner?.NotifyHostedResourcesChanged(ResourcesChangedEventArgs.Empty);
    }
}