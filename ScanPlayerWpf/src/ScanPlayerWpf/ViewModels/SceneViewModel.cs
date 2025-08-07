using GalaSoft.MvvmLight.Ioc;
using ScanPlayerWpf.Models;

namespace ScanPlayerWpf.ViewModels
{
    public class SceneViewModel : DockWindowViewModel
    {
        public SceneViewModel() : base("SceneWindow")
        {
            Title = "Scene";
            Workspace = SimpleIoc.Default.GetInstance<Workspace>();
        }

        public Workspace Workspace { get; }
    }
}
