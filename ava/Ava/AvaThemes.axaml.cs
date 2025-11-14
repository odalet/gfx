using System;
using System.Collections.Generic;
using System.Linq;
using Ava.Themes.Palettes;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;

namespace Ava;

public sealed class AvaThemes : Styles
{
    public AvaThemes(IServiceProvider? services = null)
    {
        AvaloniaXamlLoader.Load(services, this);
        // Palettes = Resources.MergedDictionaries.OfType<PaletteResourcesCollection>().FirstOrDefault() ?? throw new InvalidOperationException(
        //     $"{nameof(AvaThemes)} was initialized without a {nameof(PaletteResourcesCollection)}.");
    }

    public static ThemeVariant Default { get; } = ThemeVariant.Default;
    public static ThemeVariant Light { get; } = ThemeVariant.Light;
    public static ThemeVariant Dark { get; } = ThemeVariant.Dark;
    public static ThemeVariant HighContrast { get; } = new(nameof(HighContrast), ThemeVariant.Dark);

    public IDictionary<ThemeVariant, PaletteResources> Palettes { get; }
}