using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ScanPlayer.Views.Tools
{
    public partial class HeadsInfoView : UserControl
    {
        public HeadsInfoView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
