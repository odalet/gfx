using GalaSoft.MvvmLight.Ioc;
using ScanPlayerWpf.Models;

namespace ScanPlayerWpf.ViewModels
{
    public class SceneOptionsViewModel : DockWindowViewModel
    {
        public SceneOptionsViewModel() : base("SceneOptionsWindow")
        {
            Title = "Scene Options";
            Workspace = SimpleIoc.Default.GetInstance<Workspace>();
        }

        public Workspace Workspace { get; }
    }
}
