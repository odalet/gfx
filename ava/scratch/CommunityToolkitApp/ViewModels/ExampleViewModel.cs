using System.Diagnostics;
using Avalonia;

namespace CommunityToolkitApp.ViewModels;

public class ExampleViewModel : ViewModelBase
{
    public ExampleViewModel()
    {
        Debug.WriteLine(
            "######################## ExampleViewModel");
        Application.Current?.ActualThemeVariantChanged +=
            (_, _) => OnPropertyChanged(nameof(CurrentThemeName));
    }


    public string? CurrentThemeName => Application.Current?.ActualThemeVariant?.ToString();
}
