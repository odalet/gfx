using System;
using System.Collections.Generic;
using System.Reflection;
using Avalonia;
using Avalonia.OpenGL;
using Avalonia.ReactiveUI;
using NLog;
using ScanPlayer.Logging;

namespace ScanPlayer
{
    internal sealed partial class Program
    {
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();
        private static bool useCustomPlatformOptions;

        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static int Main(string[] args)
        {
            log.Debug("Entering Main");
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                var ex = (Exception)e.ExceptionObject;
                log.Fatal(ex, $"Unhandled application error: {ex.Message}");
            };

            useCustomPlatformOptions = true;
            var rc = BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
            log.Debug($"Exiting Main (RC = {rc})");
            return rc;
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
        {
            var defaultX11PlatformOptions = new X11PlatformOptions
            {
                // These are the defaults
                UseEGL = false,
                UseGpu = true,
                OverlayPopups = false,
                UseDBusMenu = false,
                UseDeferredRendering = true,
                EnableIme = null,
                EnableSessionManagement = Environment.GetEnvironmentVariable("AVALONIA_X11_USE_SESSION_MANAGEMENT") != "0",
                GlProfiles = new List<GlVersion>
                {
                    new(GlProfileType.OpenGL, 4, 0),
                    new(GlProfileType.OpenGL, 3, 2),
                    new(GlProfileType.OpenGL, 3, 0),
                    new(GlProfileType.OpenGLES, 3, 2),
                    new(GlProfileType.OpenGLES, 3, 0),
                    new(GlProfileType.OpenGLES, 2, 0)
                },
                GlxRendererBlacklist = new List<string> { "llvmpipe" },
                WmClass = Assembly.GetEntryAssembly()?.GetName()?.Name ?? "AvaloniaApplication",
                EnableMultiTouch = true,
            };

            var customX11PlatformOptions = new X11PlatformOptions
            {
                GlProfiles = new List<GlVersion>
                {
                    // In my WSL + Mesa/llvmpipe driver, the maximum supported GL Version is 3.2
                    // new GlVersion(GlProfileType.OpenGL, 3, 2)
                    new(GlProfileType.OpenGL, 3, 1) // But use 3.1 because the "old" GL primitives we use are otherwise not supported
                },
                GlxRendererBlacklist = new List<string>() // Do not blacklist llvmpipe: this is what we are left with in WSL...
            };

            var defaultWin32PlatformOptions = new Win32PlatformOptions
            {
                UseDeferredRendering = true,
                AllowEglInitialization = null,
                EnableMultitouch = true,
                OverlayPopups = false,
                UseWgl = false,
                WglProfiles = new List<GlVersion>
                {
                    new(GlProfileType.OpenGL, 4, 0),
                    new(GlProfileType.OpenGL, 3, 2)
                },
                UseWindowsUIComposition = true,
                CompositionBackdropCornerRadius = null
            };

            var customWin32PlatformOptions = new Win32PlatformOptions
            {
                AllowEglInitialization = false,
                // UseWgl enables OpenGL rendering on Windows
                // See https://github.com/AvaloniaUI/Avalonia/issues/5452
                UseWgl = true,
                WglProfiles = new List<GlVersion>
                {
                    // On my machine, I could go up to GL 4.5
                    //new GlVersion(GlProfileType.OpenGL, 3, 3)
                    new(GlProfileType.OpenGL, 3, 1) // But use 3.1 because the "old" GL primitives we use are otherwise not supported
                }
            };

            var x11PlatformOptions = useCustomPlatformOptions ? customX11PlatformOptions : defaultX11PlatformOptions;
            var win32PlatformOptions = useCustomPlatformOptions ? customWin32PlatformOptions : defaultWin32PlatformOptions;

            return AppBuilder
                .Configure<App>()
                .UsePlatformDetect()
                .With(x11PlatformOptions)
                .With(win32PlatformOptions)
                .LogToTrace()
                .LogToNLog(level: Avalonia.Logging.LogEventLevel.Warning)
                .UseReactiveUI();
        }
    }
}
