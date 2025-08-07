using System;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using Common.Logging;

namespace ScanPlayerWpf.Configuration
{
    internal sealed class MainWindowConfigurationManager
    {
        private static readonly ILog log = LogManager.GetLogger<SceneOptionsConfigurationManager>();
        
        public MainWindowConfiguration ReadMainWindowConfiguration(XDocument xdoc)
        {
            try
            {
                var root = xdoc.Descendants("main-window").FirstOrDefault();
                if (root == null) return MainWindowConfiguration.Default;

                double p(string text) => double.Parse(text, CultureInfo.InvariantCulture);
                var x = p(root.Attribute("x").Value);
                var y = p(root.Attribute("y").Value);
                var w = p(root.Attribute("width").Value);
                var h = p(root.Attribute("height").Value);
                var maximized = bool.Parse(root.Attribute("maximized").Value);

                return new MainWindowConfiguration(x, y, w, h, maximized);
            }
            catch (Exception ex)
            {
                log.Error($"Could not decode Main Window Configuration; defaulting to default values: {ex.Message}", ex);
                return MainWindowConfiguration.Default;
            }
        }

        public void WriteMainWindowConfiguration(XElement parent, MainWindowConfiguration configuration)
        {
            string w(double d) => d.ToString(CultureInfo.InvariantCulture);
            var mainWindowElement = new XElement("main-window",
                new XAttribute("x", w(configuration.X)),
                new XAttribute("y", w(configuration.Y)),
                new XAttribute("width", w(configuration.Width)),
                new XAttribute("height", w(configuration.Height)),
                new XAttribute("maximized", configuration.IsMaximized.ToString())
            );

            parent.Add(mainWindowElement);
        }
    }
}
