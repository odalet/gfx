using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ScanPlayer.Views
{
    public partial class MainStatusBarView : UserControl
    {
        public MainStatusBarView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
