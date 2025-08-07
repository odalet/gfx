using System.Windows.Input;

namespace ScanPlayer.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public MainWindowViewModel() => MainViewModel = new MainViewModel();

    public MainViewModel MainViewModel { get; }
}
