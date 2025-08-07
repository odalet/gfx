using System;
using System.Collections.Generic;
using System.Linq;
using AddUp.Ail;
using AddUp.Ail.IO;
using ScanPlayerWpf.Models;
using ScanPlayerWpf.Rendering;

namespace ScanPlayerWpf.Adapters
{
    internal sealed class AilDrawingProgram : IDrawingProgram
    {
        private sealed class AilToDrawingInstructionVisitor : BaseInstructionVisitor<DrawingInstruction?, AilToDrawingInstructionVisitor.Context>
        {
            public sealed class Context
            {
                public double X { get; set; }
                public double Y { get; set; }
                public double Z { get; set; }

                public void SetLocation(double x, double y, double z) { X = x; Y = y; Z = z; }
            }

            private bool isFirstJump = true;

            protected override DrawingInstruction? Visit(JumpInstruction jump, Context context)
            {
                // First Jump is ignored
                var instructionKind = isFirstJump ? DrawingInstructionKind.Noop : DrawingInstructionKind.Jump;

                if (isFirstJump) isFirstJump = false;

                var instruction = new DrawingInstruction(instructionKind, jump.X, jump.Y, jump.Z, TimeSpan.Zero);
                context.SetLocation(jump.X, jump.Y, jump.Z);
                return instruction;
            }

            protected override DrawingInstruction? Visit(MarkInstruction mark, Context context)
            {
                var instruction = new DrawingInstruction(DrawingInstructionKind.Mark, mark.X, mark.Y, mark.Z, TimeSpan.Zero);
                context.SetLocation(mark.X, mark.Y, mark.Z);
                return instruction;
            }

            protected override DrawingInstruction? Visit(DrillInstruction drill, Context context) =>
                new DrawingInstruction(DrawingInstructionKind.Point, context.X, context.Y, context.Z, TimeSpan.Zero);

            protected override DrawingInstruction? Visit(WaitInstruction wait, Context context) =>
                new DrawingInstruction(DrawingInstructionKind.Idle, context.X, context.Y, context.Z, TimeSpan.Zero);
        }

        private const int defaultHeadId = 1;
        private DrawingInstruction[] drawingInstructions;

        public AilDrawingProgram(IPrinterDefinition printer, AilBinaryReadResult result)
        {
            Printer = printer ?? throw new ArgumentNullException(nameof(printer));
            Result = result ?? throw new ArgumentNullException(nameof(result));            

            Translate();
        }

        public IPrinterDefinition Printer { get; }
        private AilBinaryReadResult Result { get; }        

        public IEnumerable<DrawingInstruction> GetInstructions(int headId) =>
            headId == defaultHeadId ? drawingInstructions : null;

        private void Translate()
        {
            var visitor = new AilToDrawingInstructionVisitor();
            var context = new AilToDrawingInstructionVisitor.Context();

            using (Result.Stream) drawingInstructions = Result.Stream
                    .Select(ailInstruction => visitor.VisitInstruction(ailInstruction, context))
                    .Where(drawingInstruction => drawingInstruction != null)
                    .Select(drawingInstruction => drawingInstruction.Value)
                    .ToArray();
        }
    }
}
