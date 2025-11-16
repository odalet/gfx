using System.Linq;
using Ava.Themes.Fluent;
using Ava.Themes.Simple;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using CommunityToolkitApp.ViewModels;
using CommunityToolkitApp.Views;

namespace CommunityToolkitApp;

public enum CatalogTheme { Fluent, Simple }

public partial class App : Application
{
    // Very much inspired by Avalonia's Control Catalog Example application
    private readonly Styles themeStylesContainer = [];
    private FluentTheme? fluentTheme;
    private SimpleTheme? simpleTheme;
    private IStyle? avaControlsStyle;

    public override void Initialize()
    {

        AvaloniaXamlLoader.Load(this);

        fluentTheme = (FluentTheme)Resources["FluentTheme"]!;
        simpleTheme = (SimpleTheme)Resources["SimpleTheme"]!;
        avaControlsStyle = (IStyle)Resources["AvaControls"]!;
        SetTheme(CatalogTheme.Fluent, initial: true);
        //SetTheme(CatalogTheme.Simple, initial: true);
        Styles.Add(themeStylesContainer);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(),
            };
        }

        // this.RegisterSystemThemeAwareness();
        base.OnFrameworkInitializationCompleted();
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }

    private CatalogTheme previousTheme;
    public static CatalogTheme CurrentTheme => ((App)Current!).previousTheme;

    public static void SetTheme(CatalogTheme theme, bool initial = true) =>
        SetTheme1(theme, initial);
        //SetTheme2(theme, initial);

    public static void SetTheme1(CatalogTheme theme, bool initial = true)
    {
        var app = (App)Current!;
        var prevTheme = app.previousTheme;
        app.previousTheme = theme;
        var shouldReopenWindow = !initial && prevTheme != theme;

        if (app.themeStylesContainer.Count == 0)
        {
            app.themeStylesContainer.Add(new Style());
            app.themeStylesContainer.Add(new Style());
            app.themeStylesContainer.Add(new Style());
        }

        if (theme == CatalogTheme.Fluent)
        {
            app.themeStylesContainer[0] = app.fluentTheme!;
            app.themeStylesContainer[1] = app.avaControlsStyle!;
        }
        else if (theme == CatalogTheme.Simple)
        {
            app.themeStylesContainer[0] = app.simpleTheme!;
            app.themeStylesContainer[1] = app.avaControlsStyle!;
        }

        if (shouldReopenWindow)
        {
            if (app.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
            {
                var oldWindow = desktopLifetime.MainWindow;
                var newWindow = new MainWindow();
                desktopLifetime.MainWindow = newWindow;
                newWindow.Show();
                oldWindow?.Close();
            }
            //else if (app.ApplicationLifetime is ISingleViewApplicationLifetime singleViewLifetime)
            //{
            //    singleViewLifetime.MainView = new MainView();
            //}
        }
    }

    public static void SetTheme2(CatalogTheme theme, bool initial = true)
    {
        var app = (App)Current!;
        var prevTheme = app.previousTheme;
        app.previousTheme = theme;
        var shouldReopenWindow = !initial && prevTheme != theme;

        app.Styles.Clear();
        app.Styles.Add(theme == CatalogTheme.Fluent
            ? app.fluentTheme! : app.simpleTheme!);
        app.Styles.Add(app.avaControlsStyle!);

        if (shouldReopenWindow)
        {
            if (app.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
            {
                var oldWindow = desktopLifetime.MainWindow;
                var newWindow = new MainWindow();
                desktopLifetime.MainWindow = newWindow;
                newWindow.Show();
                oldWindow?.Close();
            }
            //else if (app.ApplicationLifetime is ISingleViewApplicationLifetime singleViewLifetime)
            //{
            //    singleViewLifetime.MainView = new MainView();
            //}
        }
    }
}