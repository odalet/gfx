using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ScanPlayer.Views.Tools
{
    public partial class SceneOptionsView : UserControl
    {
        public SceneOptionsView() => InitializeComponent();
        private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
    }
}
