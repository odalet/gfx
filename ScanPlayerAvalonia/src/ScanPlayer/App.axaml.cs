using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using NLog;
using ScanPlayer.ViewModels;
using ScanPlayer.Views;

namespace ScanPlayer
{
    public class App : Application
    {
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();
        private static IClassicDesktopStyleApplicationLifetime? desktopLifetime;

        public override void Initialize()
        {
            log.Trace("Initializing application");
            AvaloniaXamlLoader.Load(this);
            log.Trace("Initialized application");
        }

        // Guaranteed to be non-null after OnFrameworkInitializationCompleted.
        public static IClassicDesktopStyleApplicationLifetime Desktop => desktopLifetime!;

        public override void OnFrameworkInitializationCompleted() => InitializeApplication();

        private void InitializeApplication()
        {
            Bootstrapper.Register();

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.Startup += OnStartup;
                desktop.Exit += OnExit;

                desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;

                var vm = new MainWindowViewModel();
                desktop.MainWindow = new MainWindow 
                {
                    DataContext = vm,
                    Width = 800,
                    Height = 600
                };

                desktopLifetime = desktop;
            }

            base.OnFrameworkInitializationCompleted();
            log.Trace("Framework Initialization Complete");
        }

        private void OnExit(object? sender, ControlledApplicationLifetimeExitEventArgs e)
        {
            log.Info("******** Exiting Application ********");
            log.Trace($"Exiting: Exit Code = {e.ApplicationExitCode}");
        }

        private void OnStartup(object? sender, ControlledApplicationLifetimeStartupEventArgs e)
        {
            log.Info("******** Starting up Application ********");
            log.Trace($"Startup: args = '{string.Join(", ", e.Args)}'");
        }
    }
}
