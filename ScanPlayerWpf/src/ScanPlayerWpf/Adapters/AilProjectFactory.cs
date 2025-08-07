using System;
using System.IO;
using ScanPlayerWpf.Models;

namespace ScanPlayerWpf.Adapters
{
    public sealed class AilProjectFactory : INeedExternalPrinterDefinitionProjectFactory
    {
        public AilProjectFactory()
        {
            Name = "AIL";
            SupportedFileExtensions = new[] { ".ailb" };
        }

        public string Name { get; }
        public string[] SupportedFileExtensions { get; }
        public IPrinterDefinition Printer { get; set; }

        public IProject Load(string filename)
        {
            if (filename == null) throw new ArgumentNullException(nameof(filename));
            if (!File.Exists(filename)) throw new FileNotFoundException(filename);

            return new AilProject(Printer, filename);
        }
    }
}
