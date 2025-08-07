using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Styling;

namespace ScanPlayer.Controls;

// Adapted from https://github.com/teast/Avalonia.GroupBox/blob/master/GroupBox/GroupBox.xaml.cs
public partial class GroupBox : UserControl, IStyleable
{
    private static readonly string defaultHeader = "GroupBox";
    private string header = defaultHeader;

    Type IStyleable.StyleKey => typeof(GroupBox);

    public GroupBox() => InitializeComponent();

    public static readonly StyledProperty<IBrush> HeaderBackgroundProperty = AvaloniaProperty.Register<GroupBox, IBrush>(nameof(HeaderBackground));
    public static readonly StyledProperty<IBrush> HeaderForegroundProperty = AvaloniaProperty.Register<GroupBox, IBrush>(nameof(HeaderForeground));
    public static readonly StyledProperty<Thickness> HeaderMarginProperty = AvaloniaProperty.Register<GroupBox, Thickness>(nameof(HeaderMargin));
    public static readonly DirectProperty<GroupBox, string> HeaderProperty = AvaloniaProperty.RegisterDirect<GroupBox, string>(
        nameof(Header), x => x.header, (x, v) => x.header = v, defaultHeader, BindingMode.TwoWay);

    public IBrush HeaderBackground
    {
        get => GetValue(HeaderBackgroundProperty);
        set => SetValue(HeaderBackgroundProperty, value);
    }

    public IBrush HeaderForeground
    {
        get => GetValue(HeaderForegroundProperty);
        set => SetValue(HeaderForegroundProperty, value);
    }

    public Thickness HeaderMargin
    {
        get => GetValue(HeaderMarginProperty);
        set => SetValue(HeaderMarginProperty, value);
    }

    public string Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
}
