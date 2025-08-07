using ScanPlayerWpf.Rendering;

namespace ScanPlayerWpf.Models
{
    public interface IProject
    {
        IPrinterDefinition Printer { get; }
        string FileName { get; }

        IDrawingProgram Translate();
    }
}
