using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ScanPlayer.Views.Documents
{
    public partial class SceneDocumentView : UserControl
    {
        public SceneDocumentView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
