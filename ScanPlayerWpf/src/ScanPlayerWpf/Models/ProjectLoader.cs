using System.Collections.Generic;
using System.Linq;
using Common.Logging;
using ScanPlayerWpf.Adapters;

namespace ScanPlayerWpf.Models
{
    public interface IProjectLoader
    {
        string FileFilter { get; }
        IPrinterDefinition CurrentPrinter { get; set; }
        bool CanLoadProject(string filename);
        IProject LoadProject(string filename);
    }

    internal sealed class ProjectLoader : IProjectLoader
    {
        private static readonly ILog log = LogManager.GetLogger<ProjectLoader>();

        public ProjectLoader()
        {
            Factories = new IProjectFactory[]
            {
                new AddprojProjectFactory(),
                new ScanJobProjectFactory(),
                new AilProjectFactory()
            };

            FileFilter = BuildFileFilter();
        }

        private IEnumerable<IProjectFactory> Factories { get; }
        
        public string FileFilter { get; }
        public IPrinterDefinition CurrentPrinter { get; set; }

        public bool CanLoadProject(string filename) => FactoryFromFileName(filename) != null;

        public IProject LoadProject(string filename)
        {
            var factory = FactoryFromFileName(filename);
            if (factory is INeedExternalPrinterDefinitionProjectFactory needPrinter)
                needPrinter.Printer = CurrentPrinter;
            return factory.Load(filename);
        }

        private string BuildFileFilter()
        {
            string makeExt(string ext) => 
                ext.StartsWith("*.") ? ext : 
                ext.StartsWith(".") ? "*" + ext : 
                "*." + ext;

            var allExtensions = new List<string>();
            var fileTypes = new List<string>();
            foreach (var factory in Factories)
            {
                var extensions = string.Join(";", factory.SupportedFileExtensions.Select(makeExt));
                allExtensions.Add(extensions);
                fileTypes.Add($"{factory.Name} Project Files ({extensions})|{extensions}");
            }

            // Append "All Files"
            fileTypes.Add("All Files|*.*");

            // Prepend the 'All project files line'
            var allExtensionsString = string.Join(";", allExtensions);
            fileTypes.Insert(0, $"All Project Files ({allExtensionsString})|{allExtensionsString}");

            return string.Join("|", fileTypes);
        }

        private IProjectFactory FactoryFromFileName(string filename)
        {
            foreach (var factory in Factories)
                foreach (var extension in factory.SupportedFileExtensions)
                    if (filename.EndsWith(extension))
                        return factory;

            return null;
        }
    }
}
