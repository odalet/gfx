using ScanPlayer.Models;
using Splat;

namespace ScanPlayer.ViewModels.Tools;

public class SceneOptionsViewModel : ViewModelBase
{
    public SceneOptionsViewModel() => Workspace = Locator.Current.GetRequiredService<Workspace>();

    public Workspace Workspace { get; }
}
