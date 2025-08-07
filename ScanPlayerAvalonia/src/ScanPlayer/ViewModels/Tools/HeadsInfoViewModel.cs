using System.Collections.Generic;
using Avalonia.Media;
using ReactiveUI;
using ScanPlayer.Models;
using Splat;

namespace ScanPlayer.ViewModels.Tools;

public class HeadsInfoViewModel : ViewModelBase
{
    private static class ControlPalette
    {
        // TODO: modify the palette when the theme changes
        // See also the palette in Rendering
        public static Color Red => Colors.LightCoral;
        public static Color Green => Colors.LightGreen;
        public static Color Blue => Colors.LightBlue;
        public static Color Yellow => Colors.LightYellow;
    }

    private readonly Dictionary<int, SolidColorBrush> headBrushes;

    public HeadsInfoViewModel()
    {
        headBrushes = InitializeHeadBrushes();

        Workspace = Locator.Current.GetRequiredService<Workspace>();
        Workspace.HeadsInfo.PropertyChanged += (s, e) =>
        {
            this.RaisePropertyChanged(nameof(Head1Exists));
            this.RaisePropertyChanged(nameof(Head2Exists));
            this.RaisePropertyChanged(nameof(Head3Exists));
            this.RaisePropertyChanged(nameof(Head4Exists));
            this.RaisePropertyChanged(nameof(Head1Active));
            this.RaisePropertyChanged(nameof(Head2Active));
            this.RaisePropertyChanged(nameof(Head3Active));
            this.RaisePropertyChanged(nameof(Head4Active));
        };
    }

    public Workspace Workspace { get; }

    public bool Head1Exists => Workspace.HeadsInfo.HasHead(1);
    public bool Head2Exists => Workspace.HeadsInfo.HasHead(2);
    public bool Head3Exists => Workspace.HeadsInfo.HasHead(3);
    public bool Head4Exists => Workspace.HeadsInfo.HasHead(4);

    public bool Head1Active
    {
        get => Workspace.HeadsInfo.IsHeadActive(1);
        set => Workspace.HeadsInfo.SetHeadActive(1, value);
    }

    public bool Head2Active
    {
        get => Workspace.HeadsInfo.IsHeadActive(2);
        set => Workspace.HeadsInfo.SetHeadActive(2, value);
    }

    public bool Head3Active
    {
        get => Workspace.HeadsInfo.IsHeadActive(3);
        set => Workspace.HeadsInfo.SetHeadActive(3, value);
    }

    public bool Head4Active
    {
        get => Workspace.HeadsInfo.IsHeadActive(4);
        set => Workspace.HeadsInfo.SetHeadActive(4, value);
    }

    public SolidColorBrush Head1Brush => headBrushes[1];
    public SolidColorBrush Head2Brush => headBrushes[2];
    public SolidColorBrush Head3Brush => headBrushes[3];
    public SolidColorBrush Head4Brush => headBrushes[4];

    private static Dictionary<int, SolidColorBrush> InitializeHeadBrushes() => new()
    {
        [1] = new(ControlPalette.Red),
        [2] = new(ControlPalette.Green),
        [3] = new(ControlPalette.Blue),
        [4] = new(ControlPalette.Yellow),
    };
}
