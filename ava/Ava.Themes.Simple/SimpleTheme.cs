using System;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;

namespace Ava.Themes.Simple;

public class SimpleTheme : Styles
{
    public SimpleTheme(IServiceProvider? sp = null) => AvaloniaXamlLoader.Load(sp, this);
}
