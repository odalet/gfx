using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;

namespace Ava.Themes.Palettes;

public sealed class PaletteResources : ResourceProvider
{
    private readonly Dictionary<string, Color> colors = new(StringComparer.InvariantCulture);
    private bool hasAccentColor;
    private Color accentColor;
    private Color accentColorDark1, accentColorDark2, accentColorDark3;
    private Color accentColorLight1, accentColorLight2, accentColorLight3;

    public static readonly DirectProperty<PaletteResources, Color> AccentProperty = AvaloniaProperty.RegisterDirect<PaletteResources, Color>(
        nameof(Accent), r => r.Accent, (r, v) => r.Accent = v);

    // @formatter:off
    public Color Accent { get => accentColor; set => SetAndRaise(AccentProperty, ref accentColor, value); }
    public Color AltHigh { get => GetColor("SystemAltHighColor"); set => SetColor("SystemAltHighColor", value); }
    public Color AltLow { get => GetColor("SystemAltLowColor"); set => SetColor("SystemAltLowColor", value); }
    public Color AltMedium { get => GetColor("SystemAltMediumColor"); set => SetColor("SystemAltMediumColor", value); }
    public Color AltMediumHigh { get => GetColor("SystemAltMediumHighColor"); set => SetColor("SystemAltMediumHighColor", value); }
    public Color AltMediumLow { get => GetColor("SystemAltMediumLowColor"); set => SetColor("SystemAltMediumLowColor", value); }
    public Color BaseHigh { get => GetColor("SystemBaseHighColor"); set => SetColor("SystemBaseHighColor", value); }
    public Color BaseLow { get => GetColor("SystemBaseLowColor"); set => SetColor("SystemBaseLowColor", value); }
    public Color BaseMedium { get => GetColor("SystemBaseMediumColor"); set => SetColor("SystemBaseMediumColor", value); }
    public Color BaseMediumHigh { get => GetColor("SystemBaseMediumHighColor"); set => SetColor("SystemBaseMediumHighColor", value); }
    public Color BaseMediumLow { get => GetColor("SystemBaseMediumLowColor"); set => SetColor("SystemBaseMediumLowColor", value); }
    public Color ChromeAltLow { get => GetColor("SystemChromeAltLowColor"); set => SetColor("SystemChromeAltLowColor", value); }
    public Color ChromeBlackHigh { get => GetColor("SystemChromeBlackHighColor"); set => SetColor("SystemChromeBlackHighColor", value); }
    public Color ChromeBlackLow { get => GetColor("SystemChromeBlackLowColor"); set => SetColor("SystemChromeBlackLowColor", value); }
    public Color ChromeBlackMedium { get => GetColor("SystemChromeBlackMediumColor"); set => SetColor("SystemChromeBlackMediumColor", value); }
    public Color ChromeBlackMediumLow { get => GetColor("SystemChromeBlackMediumLowColor"); set => SetColor("SystemChromeBlackMediumLowColor", value); }
    public Color ChromeDisabledHigh { get => GetColor("SystemChromeDisabledHighColor"); set => SetColor("SystemChromeDisabledHighColor", value); }
    public Color ChromeDisabledLow { get => GetColor("SystemChromeDisabledLowColor"); set => SetColor("SystemChromeDisabledLowColor", value); }
    public Color ChromeGray { get => GetColor("SystemChromeGrayColor"); set => SetColor("SystemChromeGrayColor", value); }
    public Color ChromeHigh { get => GetColor("SystemChromeHighColor"); set => SetColor("SystemChromeHighColor", value); }
    public Color ChromeLow { get => GetColor("SystemChromeLowColor"); set => SetColor("SystemChromeLowColor", value); }
    public Color ChromeMedium { get => GetColor("SystemChromeMediumColor"); set => SetColor("SystemChromeMediumColor", value); }
    public Color ChromeMediumLow { get => GetColor("SystemChromeMediumLowColor"); set => SetColor("SystemChromeMediumLowColor", value); }
    public Color ChromeWhite { get => GetColor("SystemChromeWhiteColor"); set => SetColor("SystemChromeWhiteColor", value); }
    public Color ErrorText { get => GetColor("SystemErrorTextColor"); set => SetColor("SystemErrorTextColor", value); }
    public Color ListLow { get => GetColor("SystemListLowColor"); set => SetColor("SystemListLowColor", value); }
    public Color ListMedium { get => GetColor("SystemListMediumColor"); set => SetColor("SystemListMediumColor", value); }
    public Color RegionColor { get => GetColor("SystemRegionColor"); set => SetColor("SystemRegionColor", value); }
    // @formatter:on
    
    public override bool HasResources => hasAccentColor || colors.Count > 0;

    public override bool TryGetResource(object key, ThemeVariant? theme, out object? value)
    {
        value = null;
        if (key is not string str)
            return false;

        if (str.Equals(SystemAccentColors.AccentKey, StringComparison.InvariantCulture))
        {
            value = accentColor;
            return true;
        }

        if (str.Equals(SystemAccentColors.AccentDark1Key, StringComparison.InvariantCulture))
        {
            value = accentColorDark1;
            return true;
        }

        if (str.Equals(SystemAccentColors.AccentDark2Key, StringComparison.InvariantCulture))
        {
            value = accentColorDark2;
            return true;
        }

        if (str.Equals(SystemAccentColors.AccentDark3Key, StringComparison.InvariantCulture))
        {
            value = accentColorDark3;
            return true;
        }

        if (str.Equals(SystemAccentColors.AccentLight1Key, StringComparison.InvariantCulture))
        {
            value = accentColorLight1;
            return true;
        }

        if (str.Equals(SystemAccentColors.AccentLight2Key, StringComparison.InvariantCulture))
        {
            value = accentColorLight2;
            return true;
        }

        if (str.Equals(SystemAccentColors.AccentLight3Key, StringComparison.InvariantCulture))
        {
            value = accentColorLight3;
            return true;
        }

        return false;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property != AccentProperty)
            return;

        hasAccentColor = accentColor != default;
        if (hasAccentColor)
            (accentColorDark1, accentColorDark2, accentColorDark3, accentColorLight1, accentColorLight2, accentColorLight3) =
                SystemAccentColors.CalculateAccentShades(accentColor);

        RaiseResourcesChanged();
    }
    
    private Color GetColor(string key) => colors.GetValueOrDefault(key);
    private void SetColor(string key, Color value)
    {
        if (value == default)
            colors.Remove(key);
        else
            colors[key] = value;
    }
}