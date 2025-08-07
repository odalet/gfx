using ScanPlayerWpf.Models;

namespace ScanPlayerWpf.Adapters
{
    public sealed class AddprojProjectFactory : IProjectFactory
    {
        public AddprojProjectFactory()
        {
            Name = "Addproj";
            SupportedFileExtensions = new[] { ".addproj" };
        }

        public string Name { get; }
        public string[] SupportedFileExtensions { get; }

        public IProject Load(string filename) => null;
    }
}
