using System;
using System.Collections.Generic;
using ScanPlayerWpf.Models;

namespace ScanPlayerWpf.Rendering
{
    public enum DrawingInstructionKind
    {
        Jump,
        Mark,
        Point,
        Idle,
        Noop
    }

    public readonly struct DrawingInstruction
    {
        public DrawingInstruction(DrawingInstructionKind kind, double x, double y, double z, TimeSpan duration)
        {
            Kind = kind;
            X = x;
            Y = y;
            Z = z;
            Duration = duration;
        }

        public DrawingInstructionKind Kind { get; }
        public double X { get; }
        public double Y { get; }
        public double Z { get; }
        public TimeSpan Duration { get; }
    }

    public interface IDrawingProgram
    {
        IPrinterDefinition Printer { get; }
        IEnumerable<DrawingInstruction> GetInstructions(int headId);
    }
}
