using System.Linq;
using System.Windows.Input;
using Avalonia.Controls;
using ReactiveUI;
using ScanPlayer.Controls;
using ScanPlayer.Models;
using ScanPlayer.ViewModels.Documents;
using ScanPlayer.ViewModels.Tools;
using Splat;

namespace ScanPlayer.ViewModels;

public sealed class MainViewModel : ViewModelBase
{
    private string? currentView;

    public MainViewModel()
    {
        OpenCommand = ReactiveCommand.Create(Open);
        ExitCommand = ReactiveCommand.Create(Exit);

        Workspace = Locator.Current.GetRequiredService<Workspace>();

        //MainMenuViewModel = new();
        HeadsInfoViewModel = new();
        SceneOptionsViewModel = new();
        SceneDocumentViewModel = new();
        TeapotDocumentViewModel = new();
    }

    public ICommand OpenCommand { get; }
    public ICommand ExitCommand { get; }

    public Workspace Workspace { get; }

    public SceneOptionsViewModel SceneOptionsViewModel { get; }
    public HeadsInfoViewModel HeadsInfoViewModel { get; }
    public SceneDocumentViewModel SceneDocumentViewModel { get; }
    public TeapotDocumentViewModel TeapotDocumentViewModel { get; }

    public string? CurrentView
    {
        get => currentView;
        set => this.RaiseAndSetIfChanged(ref currentView, value);
    }

    private async void Open()
    {
        var dlg = new OpenFileDialog() { Title = "Open" };
        dlg.Filters.Add(new FileDialogFilter() { Name = "ScanJob Files", Extensions = { "scanjob" } });
        dlg.Filters.Add(new FileDialogFilter() { Name = "All", Extensions = { "*" } });
        var result = await dlg.ShowAsync(App.Desktop!.MainWindow);
        var item = result?.FirstOrDefault() ?? "";

        InformationBox.Show($"Opened {item}");
    }

    private void Exit() => App.Desktop!.MainWindow.Close();
}

