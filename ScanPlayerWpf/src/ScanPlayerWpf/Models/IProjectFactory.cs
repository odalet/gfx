namespace ScanPlayerWpf.Models
{
    public interface IProjectFactory
    {
        string Name { get; }
        string[] SupportedFileExtensions { get; }

        IProject Load(string filename);
    }

    public interface INeedExternalPrinterDefinitionProjectFactory : IProjectFactory
    {
        IPrinterDefinition Printer { get; set; }
    }
}
