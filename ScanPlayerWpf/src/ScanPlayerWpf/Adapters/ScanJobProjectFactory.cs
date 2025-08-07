using ScanPlayerWpf.Models;

namespace ScanPlayerWpf.Adapters
{
    public sealed class ScanJobProjectFactory : IProjectFactory
    {
        public ScanJobProjectFactory()
        {
            Name = "ScanJob";
            SupportedFileExtensions = new[] { ".scanjob", ".xml" };
        }

        public string Name { get; }
        public string[] SupportedFileExtensions { get; }

        public IProject Load(string filename) => null;
    }
}
