using System.Windows.Input;
using Ava.Theming;
using Avalonia;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.Input;

namespace CommunityToolkitApp.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public MainWindowViewModel()
    {
        SelectDefaultThemeCommand = new RelayCommand(() => SelectTheme(ThemingUtils.Default));
        SelectLightThemeCommand = new RelayCommand(() => SelectTheme(ThemingUtils.Light));
        SelectDarkThemeCommand = new RelayCommand(() => SelectTheme(ThemingUtils.Dark));
        SelectHighContrastThemeCommand = new RelayCommand(() => SelectTheme(ThemingUtils.HighContrast));
    }

    public ICommand SelectDefaultThemeCommand { get; }
    public ICommand SelectLightThemeCommand { get; }
    public ICommand SelectDarkThemeCommand { get; }
    public ICommand SelectHighContrastThemeCommand { get; }

    public string? CurrentThemeName =>
        Application.Current?.ActualThemeVariant?.ToString();

    public ExampleViewModel ExampleViewModel { get; } = new();


    private void SelectTheme(ThemeVariant theme)
    {
        var app = Application.Current;
        if (app is null) return;
        
        app.RequestedThemeVariant = theme;

        if (theme == ThemeVariant.Default)
            app.RegisterSystemThemeAwareness();
        else
            app.UnregisterSystemThemeAwareness();

        OnPropertyChanged(nameof(CurrentThemeName));
    }
}