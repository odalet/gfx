using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ScanPlayer.Views.Documents
{
    public partial class TeapotDocumentView : UserControl
    {
        public TeapotDocumentView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
