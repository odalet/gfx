using System;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;

namespace Ava;

public class TestTheme : Styles
{
    public TestTheme(IServiceProvider? services = null)
    {
        AvaloniaXamlLoader.Load(services, this);
        // Palettes = Resources.MergedDictionaries.OfType<PaletteResourcesCollection>().FirstOrDefault() ?? throw new InvalidOperationException(
        //     $"{nameof(AvaThemes)} was initialized without a {nameof(PaletteResourcesCollection)}.");
    }
}