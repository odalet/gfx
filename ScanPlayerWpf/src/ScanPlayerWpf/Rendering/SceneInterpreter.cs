using System;
using System.Collections.Generic;
using HelixToolkit.Wpf.SharpDX;
using HelixToolkit.Wpf.SharpDX.Model;
using HelixToolkit.Wpf.SharpDX.Model.Scene;
using ScanPlayerWpf.Models;
using SharpDX;

namespace ScanPlayerWpf.Rendering
{
    internal sealed class SceneInterpreter
    {
        private class SceneContext
        {
            public double X;
            public double Y;
            public double Z;
            public TimeSpan Now;
        }

        private LineBuilder jumpsBuilder;
        private LineBuilder marksBuilder;
        private LineBuilder pointsBuilder;

        public SceneInterpreter(TimeSpan maxTime, IScene scene, SceneNodeGroupModel3D target)
        {
            Scene = scene ?? throw new ArgumentNullException(nameof(scene));
            MaxTime = maxTime;
            Target = target;
        }

        private SceneNodeGroupModel3D Target { get; }
        private TimeSpan MaxTime { get; }
        private IScene Scene { get; }
        private ISceneOptions Options => Scene.Options;
        private IPrinterDefinition Printer => Scene.Printer;
        private IDrawingProgram Program => Scene.Program;
        private SceneContext Context { get; set; }

        public void Execute()
        {
            foreach (var head in Program.Printer.Heads)
            {
                ////if (!Options.IsHeadEnabled(head.Id))
                ////    continue;

                var instructions = Program.GetInstructions(head.Id);
                if (instructions == null)
                    continue;

                // Initialize the builders
                marksBuilder = new LineBuilder();
                jumpsBuilder = new LineBuilder();
                pointsBuilder = new LineBuilder();

                DrawInstructions(instructions);

                var marksColor = Palette.GetColor4(head.PreferredColorIndex, PaletteStyle.Dark);
                var jumpsColor = Palette.GetColor4(head.PreferredColorIndex, 0.75f, PaletteStyle.Dark);

                var jumps = new LineNode
                {
                    Name = NodeNames.Jumps,
                    Visible = false,
                    Geometry = jumpsBuilder.ToLineGeometry3D(),
                    Material = new LineMaterialCore
                    {
                        LineColor = jumpsColor,
                        Texture = ResourceHelper.CreateDotTexture(),
                        Thickness = 1f
                    }
                };

                var marks = new LineNode
                {
                    Name = NodeNames.Marks,
                    Visible = false,
                    Geometry = marksBuilder.ToLineGeometry3D(),
                    Material = new LineMaterialCore
                    {
                        LineColor = marksColor,
                        Thickness = 1f
                    }
                };

                var points = new LineNode
                {
                    Name = NodeNames.Points,
                    Visible = false,
                    Geometry = pointsBuilder.ToLineGeometry3D(),
                    Material = new LineMaterialCore
                    {
                        LineColor = marksColor,
                        Thickness = 1f
                    }
                };

                var headNode = new GroupNode { Name = NodeNames.GetHeadNodeName(head.Id) };
                _ = headNode.AddChildNode(jumps);
                _ = headNode.AddChildNode(marks);
                _ = headNode.AddChildNode(points);

                Target.AddNode(headNode);
            }
        }

        private void DrawInstructions(IEnumerable<DrawingInstruction> instructions)
        {
            Context = new SceneContext();
            foreach (var instruction in instructions)
            {
                if (Context.Now + instruction.Duration > MaxTime)
                    break;

                DrawInstruction(instruction);
            }
        }

        private void DrawInstruction(DrawingInstruction instruction)
        {
            switch (instruction.Kind)
            {
                case DrawingInstructionKind.Jump:
                    DrawJump(instruction);
                    break;
                case DrawingInstructionKind.Mark:
                    DrawMark(instruction);
                    break;
                case DrawingInstructionKind.Point:
                    DrawPoint(instruction);
                    break;
                case DrawingInstructionKind.Idle:
                    // Draw nothing for now, just wait
                    break;
                case DrawingInstructionKind.Noop:
                    // Do nothing
                    break;
            }

            UpdateContext(instruction);
        }

        private void DrawJump(DrawingInstruction instruction) => DrawMarkOrJump(jumpsBuilder, instruction);
        private void DrawMark(DrawingInstruction instruction) => DrawMarkOrJump(marksBuilder, instruction);
        private void DrawMarkOrJump(LineBuilder builder, DrawingInstruction instruction) => builder.AddLine(
            new Vector3((float)Context.X, (float)Context.Y, (float)Context.Z),
            new Vector3((float)instruction.X, (float)instruction.Y, (float)instruction.Z));

        private void DrawPoint(DrawingInstruction instruction) => pointsBuilder.AddBox(
            new Vector3((float)instruction.X, (float)instruction.Y, (float)instruction.Z),
            0.1f, 0.1f, 0.0001f);

        private void UpdateContext(DrawingInstruction instruction)
        {
            Context.X = instruction.X;
            Context.Y = instruction.Y;
            Context.Z = instruction.Z;
            Context.Now += instruction.Duration;
        }
    }
}
