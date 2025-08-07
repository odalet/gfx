////////using System.Globalization;
////////using System.Xml;

////////namespace ScanPlayerWpf.Configuration
////////{
////////    internal class ProfileConfiguration
////////    {
////////        public double MainWindowX { get; set; }
////////        public double MainWindowY { get; set; }
////////        public double MainWindowWidth { get; set; }
////////        public double MainWindowHeight { get; set; }
////////        public bool MainWindowIsMaximized { get; set; }

////////        public void Save(XmlDocument document)
////////        {
////////            var xAttribute = document.CreateAttribute("x");
////////            xAttribute.Value = MainWindowX.ToString(CultureInfo.InvariantCulture);

////////            var yAttribute = document.CreateAttribute("x");
////////            yAttribute.Value = MainWindowY.ToString(CultureInfo.InvariantCulture);

////////            var wAttribute = document.CreateAttribute("width");
////////            wAttribute.Value = MainWindowWidth.ToString(CultureInfo.InvariantCulture);

////////            var hAttribute = document.CreateAttribute("height");
////////            hAttribute.Value = MainWindowHeight.ToString(CultureInfo.InvariantCulture);

////////            var mAttribute = document.CreateAttribute("maximized");
////////            mAttribute.Value = MainWindowIsMaximized.ToString(CultureInfo.InvariantCulture);

////////            var mainWindowChild = document.CreateElement("mainWindow");
////////            mainWindowChild.Attributes.Append(xAttribute);
////////            mainWindowChild.Attributes.Append(yAttribute);
////////            mainWindowChild.Attributes.Append(wAttribute);
////////            mainWindowChild.Attributes.Append(hAttribute);
////////            mainWindowChild.Attributes.Append(mAttribute);

////////            document.AppendChild(mainWindowChild);
////////        }

////////        public void Load(XmlDocument document)
////////        {
////////            var root = document.SelectSingleNode("//mainWindow");
////////            MainWindowX = double.Parse(root.Attributes["x"].Value, CultureInfo.InvariantCulture);
////////            MainWindowY = double.Parse(root.Attributes["y"].Value, CultureInfo.InvariantCulture);
////////            MainWindowWidth = double.Parse(root.Attributes["width"].Value, CultureInfo.InvariantCulture);
////////            MainWindowHeight = double.Parse(root.Attributes["height"].Value, CultureInfo.InvariantCulture);
////////            MainWindowIsMaximized = bool.Parse(root.Attributes["maximized"].Value);
////////        }
////////    }
////////}
