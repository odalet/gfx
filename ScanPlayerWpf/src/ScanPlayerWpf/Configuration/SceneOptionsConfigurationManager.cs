using System;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using Common.Logging;
using ScanPlayerWpf.Models;

namespace ScanPlayerWpf.Configuration
{
    internal sealed class SceneOptionsConfigurationManager
    {
        private static readonly ILog log = LogManager.GetLogger<SceneOptionsConfigurationManager>();

        public SceneOptions ReadSceneOptions(XDocument xdoc)
        {
            var sceneOptions = new SceneOptions(); // Filled with default values
            try
            {
                var root = xdoc.Descendants("scene").FirstOrDefault();
                if (root == null) return sceneOptions;

                void read(string attribute, Action<bool> setValue)
                {
                    try
                    {
                        setValue(bool.Parse(root.Attribute(attribute).Value));
                    }
                    catch
                    {
                        log.Trace($"Could not parse attribute '{attribute}'");
                    }
                }

                read("show-platform", x => sceneOptions.ShowPlatform = x);
                read("show-head-references", x => sceneOptions.ShowHeadReferences = x);
                read("show-head-fields", x => sceneOptions.ShowHeadFields = x);
                read("show-reference", x => sceneOptions.ShowReference = x);
                read("show-jumps", x => sceneOptions.ShowJumps = x);
                read("show-marks", x => sceneOptions.ShowMarks = x);
                read("show-points", x => sceneOptions.ShowPoints = x);
                read("show-hulls", x => sceneOptions.ShowHulls = x);
            }
            catch (Exception ex)
            {
                log.Error($"Could not read Scene Options from configuration; defaulting to default values: {ex.Message}", ex);
            }

            return sceneOptions;
        }

        public void WriteSceneOptions(XElement parent, SceneOptions sceneOptions)
        {
            string w(bool b) => b.ToString(CultureInfo.InvariantCulture);
            var mainWindowElement = new XElement("scene",
                new XAttribute("show-platform", w(sceneOptions.ShowPlatform)),
                new XAttribute("show-head-references", w(sceneOptions.ShowHeadReferences)),
                new XAttribute("show-head-fields", w(sceneOptions.ShowHeadFields)),
                new XAttribute("show-reference", w(sceneOptions.ShowReference)),
                new XAttribute("show-jumps", w(sceneOptions.ShowJumps)),
                new XAttribute("show-marks", w(sceneOptions.ShowMarks)),
                new XAttribute("show-points", w(sceneOptions.ShowPoints)),
                new XAttribute("show-hulls", w(sceneOptions.ShowHulls))
            );

            parent.Add(mainWindowElement);
        }
    }
}
