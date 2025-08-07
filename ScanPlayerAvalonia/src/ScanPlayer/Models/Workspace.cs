using ReactiveUI;

namespace ScanPlayer.Models;


public sealed class Workspace : ReactiveObject
{
    public Workspace()
    {
        SceneOptions = new();
        HeadsInfo = new();
    }

    public SceneOptions SceneOptions { get; }
    public HeadsInfo HeadsInfo { get; }
}

