using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using ScanPlayerWpf.ViewModels;

namespace ScanPlayerWpf
{
    public sealed class ViewModelLocator
    {
        private ViewModelLocator()
        {
            if (ViewModelBase.IsInDesignModeStatic) // NB: at runtime, the main window model is built in the window's constructor
                SimpleIoc.Default.Register(() => new MainWindowModel(null));

            SimpleIoc.Default.Register<SceneViewModel>();
            SimpleIoc.Default.Register<SceneOptionsViewModel>();
            SimpleIoc.Default.Register<HeadsViewModel>();
            SimpleIoc.Default.Register<TimeViewModel>();
            SimpleIoc.Default.Register<LogViewModel>();
        }

        public static void Initialize() => Current = new ViewModelLocator();

        public static ViewModelLocator Current { get; private set; }

        public MainWindowModel MainWindowModel => SimpleIoc.Default.GetInstance<MainWindowModel>();
        public SceneViewModel SceneViewModel => SimpleIoc.Default.GetInstance<SceneViewModel>();
        public SceneOptionsViewModel SceneOptionsViewModel => SimpleIoc.Default.GetInstance<SceneOptionsViewModel>();
        public HeadsViewModel HeadsViewModel => SimpleIoc.Default.GetInstance<HeadsViewModel>();
        public TimeViewModel TimeViewModel => SimpleIoc.Default.GetInstance<TimeViewModel>();
        public LogViewModel LogViewModel => SimpleIoc.Default.GetInstance<LogViewModel>();

        public static void Cleanup()
        {
            // Nothing to do here... for now
        }
    }
}