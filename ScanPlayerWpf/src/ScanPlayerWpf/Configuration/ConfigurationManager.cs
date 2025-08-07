using System;
using System.IO;
using System.Xml.Linq;
using AvalonDock;
using AvalonDock.Layout.Serialization;
using Common.Logging;
using GalaSoft.MvvmLight.Ioc;
using ScanPlayerWpf.Models;

namespace ScanPlayerWpf.Configuration
{
    internal interface IConfigurationManager
    {
        UserConfiguration LoadUserConfiguration();
        void LoadLayout(DockingManager dockingManager);
        void SaveUserConfiguration(UserConfiguration configuration);
        void SaveLayout(DockingManager dockingManager);
    }

    internal class ConfigurationManager : IConfigurationManager
    {
        private static readonly ILog log = LogManager.GetLogger<ConfigurationManager>();

        private string UserConfigurationFileName => Path.Combine(
            Constants.UserDataFolder, "user.config");

        private string LayoutConfigurationFileName => Path.Combine(
            Constants.UserDataFolder, "layout.config");

        public UserConfiguration LoadUserConfiguration()
        {
            var filename = UserConfigurationFileName;
            var defaultConfiguration = new UserConfiguration();

            if (!File.Exists(filename))
            {
                log.Warn($"No User Configuration file ({filename})");
                return defaultConfiguration;
            }

            try
            {
                var xdoc = XDocument.Load(filename);
                return ReadUserConfiguration(xdoc);
            }
            catch (Exception ex)
            {
                log.Error($"Could not load User Configuration from {filename}: {ex.Message}");
                return defaultConfiguration;
            }
        }

        public void SaveUserConfiguration(UserConfiguration configuration)
        {
            var data = configuration ?? new UserConfiguration(); // default
            var filename = UserConfigurationFileName;

            try
            {
                var xdoc = new XDocument();
                WriteUserConfiguration(data, xdoc);
                SaveXDocument(xdoc, filename);
            }
            catch (Exception ex)
            {
                log.Error($"Could not save User Configuration to {filename}: {ex.Message}");
            }
        }

        public void LoadLayout(DockingManager dockingManager)
        {
            if (File.Exists(LayoutConfigurationFileName))
                LoadLayoutFromFile(dockingManager);
            else
            {
                log.Warn($"No Layout Configuration File ({LayoutConfigurationFileName}); loading default configuration.");
                LoadLayoutFromResource(dockingManager);
            }
        }

        public void SaveLayout(DockingManager dockingManager)
        {
            try
            {
                var serializer = new XmlLayoutSerializer(dockingManager);
                serializer.Serialize(LayoutConfigurationFileName);
            }
            catch (Exception ex)
            {
                log.Error($"Could not save Current Layout to {LayoutConfigurationFileName}: {ex.Message}", ex);
            }
        }

        private void LoadLayoutFromFile(DockingManager dockingManager)
        {
            try
            {
                var serializer = new XmlLayoutSerializer(dockingManager);
                serializer.Deserialize(LayoutConfigurationFileName);
            }
            catch (Exception ex)
            {
                log.Error($"Could not load Current Layout from {LayoutConfigurationFileName}: {ex.Message}", ex);
            }
        }

        private void LoadLayoutFromResource(DockingManager dockingManager)
        {
            try
            {
                var serializer = new XmlLayoutSerializer(dockingManager);
                using (var reader = new StringReader(Resources.DefaultLayout))
                    serializer.Deserialize(reader);
            }
            catch (Exception ex)
            {
                log.Error($"Could not load Current Layout from resources: {ex.Message}", ex);
            }
        }

        private UserConfiguration ReadUserConfiguration(XDocument xdoc)
        {
            var configuration = new UserConfiguration();
            var mainWindowConfigurationManager = new MainWindowConfigurationManager();
            var mainWindowConfiguration = mainWindowConfigurationManager.ReadMainWindowConfiguration(xdoc);
            configuration.MainWindowConfiguration = mainWindowConfiguration;

            // Load Workspace-related objects
            var workspace = SimpleIoc.Default.GetInstance<Workspace>();

            var sceneOptionsConfigurationManager = new SceneOptionsConfigurationManager();
            var sceneOptions = sceneOptionsConfigurationManager.ReadSceneOptions(xdoc);
            workspace.SceneOptions = sceneOptions;

            return configuration;
        }

        private void WriteUserConfiguration(UserConfiguration configuration, XDocument xdoc)
        {
            var root = new XElement("user-config");

            var mainWindowConfigurationManager = new MainWindowConfigurationManager();
            mainWindowConfigurationManager.WriteMainWindowConfiguration(root, configuration.MainWindowConfiguration);

            var workspace = SimpleIoc.Default.GetInstance<Workspace>();
            var sceneOptionsConfigurationManager = new SceneOptionsConfigurationManager();
            sceneOptionsConfigurationManager.WriteSceneOptions(root, workspace.SceneOptions);

            xdoc.Add(root);
        }

        private void SaveXDocument(XDocument xdoc, string filename)
        {
            var directory = Path.GetDirectoryName(filename);
            if (!Directory.Exists(directory))
                _ = Directory.CreateDirectory(directory);

            xdoc.Save(filename);
        }
    }
}
