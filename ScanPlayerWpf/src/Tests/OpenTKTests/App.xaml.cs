using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using OpenTKTests.Rendering;

namespace OpenTKTests
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            Application.Current.Exit += (s, e) => RenderingEngine.Uninitalize();
            RenderingEngine.Initialize();
        }
    }
}
