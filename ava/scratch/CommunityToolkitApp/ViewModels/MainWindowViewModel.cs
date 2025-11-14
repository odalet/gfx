using System.Windows.Input;
using Ava;
using Avalonia;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.Input;

namespace CommunityToolkitApp.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public MainWindowViewModel()
    {
        SelectDefaultThemeCommand = new RelayCommand(() => SelectTheme(AvaThemes.Default));
        SelectLightThemeCommand = new RelayCommand(() => SelectTheme(AvaThemes.Light));
        SelectDarkThemeCommand = new RelayCommand(() => SelectTheme(AvaThemes.Dark));
        SelectHighContrastThemeCommand = new RelayCommand(() => SelectTheme(AvaThemes.HighContrast));
    }

    public ICommand SelectDefaultThemeCommand { get; }
    public ICommand SelectLightThemeCommand { get; }
    public ICommand SelectDarkThemeCommand { get; }
    public ICommand SelectHighContrastThemeCommand { get; }

    public string? CurrentThemeName =>
        Application.Current?.ActualThemeVariant?.ToString();
    
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