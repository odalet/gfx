using ScanPlayer.ViewModels.Documents;
using ScanPlayer.ViewModels.Tools;

namespace ScanPlayer.ViewModels;

public static class DesignerViewModels
{
    static DesignerViewModels()
    {
        Bootstrapper.Register();

        MainWindowViewModel = new();
        MainViewModel = new();
        SceneOptionsViewModel = new();
        HeadsInfoViewModel = new();
        TeapotDocumentViewModel = new();
        SceneDocumentViewModel = new();
    }

    public static MainWindowViewModel MainWindowViewModel { get; }
    public static MainViewModel MainViewModel { get; }
    public static SceneOptionsViewModel SceneOptionsViewModel { get; }
    public static HeadsInfoViewModel HeadsInfoViewModel { get; }
    public static TeapotDocumentViewModel TeapotDocumentViewModel { get; }
    public static SceneDocumentViewModel SceneDocumentViewModel { get; }
}
