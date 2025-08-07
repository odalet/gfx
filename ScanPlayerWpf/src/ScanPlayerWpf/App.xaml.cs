using System.Windows;
using Common.Logging;
using GalaSoft.MvvmLight.Ioc;
using ScanPlayerWpf.Configuration;
using ScanPlayerWpf.Models;

namespace ScanPlayerWpf
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static readonly ILog log = LogManager.GetLogger<App>();

        public App()
        {
            ViewModelLocator.Initialize();
            InitializeComponent();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            log.Info("******** Starting up Application ********");
            base.OnStartup(e);

            CreateServices();

            LoadConfiguration();
            
            ShutdownMode = ShutdownMode.OnExplicitShutdown;

            MainWindow = CreateMainWindow();
            MainWindow.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            log.Info("******** Exiting Application ********");
            SaveConfiguration();
            base.OnExit(e);
        }

        private Window CreateMainWindow()
        {
            var window = new MainWindow();
            return window;
        }

        private void CreateServices()
        {
            SimpleIoc.Default.Register<Workspace>();
            SimpleIoc.Default.Register<IConfigurationManager>(() => new ConfigurationManager(), true);            
        }

        private void LoadConfiguration()
        {
            var manager = SimpleIoc.Default.GetInstance<IConfigurationManager>();
            var userConfiguration = manager.LoadUserConfiguration();
            SimpleIoc.Default.Register(() => userConfiguration, true);
        }

        private void SaveConfiguration()
        {
            var manager = SimpleIoc.Default.GetInstance<IConfigurationManager>();
            var userConfiguration = SimpleIoc.Default.GetInstance<UserConfiguration>();
            manager.SaveUserConfiguration(userConfiguration);
        }
    }
}
