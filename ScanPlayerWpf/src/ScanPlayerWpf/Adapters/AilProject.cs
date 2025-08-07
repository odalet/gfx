using System;
using AddUp.Ail.IO;
using ScanPlayerWpf.Models;
using ScanPlayerWpf.Rendering;

namespace ScanPlayerWpf.Adapters
{
    internal sealed class AilProject : IProject
    {
        public AilProject(IPrinterDefinition printer, string filename)
        {
            Printer = printer ?? throw new ArgumentNullException(nameof(printer));
            FileName = filename;
        }

        public IPrinterDefinition Printer { get; }
        public string FileName { get; }

        public IDrawingProgram Translate()
        {
            var reader = new AilBinaryFactory().CreateReader();
            var result = reader.Read(FileName);
            return new AilDrawingProgram(Printer, result);
        }
    }
}
