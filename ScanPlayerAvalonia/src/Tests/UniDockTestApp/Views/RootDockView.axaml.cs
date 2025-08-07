using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace UniDockTestApp.Views
{
    public partial class RootDockView : UserControl
    {
        public RootDockView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
