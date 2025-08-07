using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Media;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using ScanPlayerWpf.Models;
using ScanPlayerWpf.Rendering;

namespace ScanPlayerWpf.ViewModels
{
    public sealed class HeadViewModel : ViewModelBase
    {
        public HeadViewModel(ISceneOptions sceneOptions, int id, int colorIndex) : this(sceneOptions, id, colorIndex, $"Head #{id}") { }
        public HeadViewModel(ISceneOptions sceneOptions, int id, int colorIndex, string name)
        {
            SceneOptions = sceneOptions;
            Id = id;
            Background = GetBrush(colorIndex);
            Name = name;
            IsEnabled = SceneOptions.IsHeadEnabled(Id);
        }

        public int Id { get; }
        public string Name { get; }
        public Brush Background { get; }
        private ISceneOptions SceneOptions { get; }

        private bool isEnabled;
        public bool IsEnabled
        {
            get => isEnabled;
            set
            {
                if (Set(ref isEnabled, value))
                    SceneOptions.EnableHead(Id, IsEnabled);
            }
        }

        private static Brush GetBrush(int index)
        {
            var rgb = Palette.GetRgbBytes(index, PaletteStyle.Normal);
            return new SolidColorBrush(Color.FromRgb(rgb.r, rgb.g, rgb.b));
        }
    }

    public class HeadsViewModel : DockWindowViewModel
    {
        public static HeadsViewModel DesignTime { get; } = new HeadsViewModel(true);

        [PreferredConstructor]
        public HeadsViewModel() : this(false) { }
        private HeadsViewModel(bool designMode) : base("HeadsWindow")
        {
            Title = "Heads";

            if (designMode)
            {
                var options = new SceneOptions();
                options.EnableHead(3, false);
                options.EnableHead(4, false);

                Heads = new[]
                {
                    new HeadViewModel(options, 1, 0),
                    new HeadViewModel(options, 2, 1),
                    new HeadViewModel(options, 3, 2, "h3"),
                    new HeadViewModel(options, 4, 3, "h4")
                };
            }
            else
            {
                Workspace = SimpleIoc.Default.GetInstance<Workspace>();
                Workspace.PropertyChanged += OnWorkspacePropertyChanged;
                UpdateHeads();
            }
        }

        private Workspace Workspace { get; }

        private HeadViewModel[] heads;
        public HeadViewModel[] Heads
        {
            get => heads;
            set => Set(ref heads, value);
        }

        private void OnWorkspacePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(Workspace.Printer)) return;
            UpdateHeads();
        }

        private void UpdateHeads() => Heads = Workspace.Printer.Heads
            .Select(h => new HeadViewModel(Workspace.SceneOptions, h.Id, h.PreferredColorIndex, $"{h.Name} ({h.Id})"))
            .ToArray();
    }
}
