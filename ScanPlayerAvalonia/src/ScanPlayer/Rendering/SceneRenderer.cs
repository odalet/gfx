using System;
using AddUp.NCore.Printing.Catalog;
using AddUp.NCore.Printing.ProjectModel;
using AddUp.NCore.Scan;
using AddUp.NCore.Scan.Geometry;
using AddUp.NCore.Scan.Trajectories;
using ScanPlayer.Controls;
using ScanPlayer.Models;
using ScanPlayer.OpenGL;

namespace ScanPlayer.Rendering;

internal sealed class SceneRenderer : BaseRenderer
{
    public SceneRenderer(GLSurface surface, Scene scene, SceneOptions sceneOptions) : base(surface)
    {
        Scene = scene;
        Scene.PropertyChanged += (s, e) => Invalidate();
        SceneOptions = sceneOptions;
        SceneOptions.PropertyChanged += (s, e) => Invalidate();
    }

    protected override bool SupportsTrackballRotation => false;

    private Scene Scene { get; }
    private SceneOptions SceneOptions { get; }

    protected override (byte r, byte g, byte b, byte a) InitialClearColor => (0x28, 0x28, 0x28, 0xFF);

    protected override void OnRenderScene(GL gl)
    {
        if (SceneOptions.DrawReference) DrawReference(gl, 0.2 / ScalingRatio);

        if (SceneOptions.DrawPlatform) DrawPlatform(gl);
        if (SceneOptions.DrawHeads) DrawHeads(gl);
        if (SceneOptions.DrawHeadFields) DrawHeadFields(gl);
        if (SceneOptions.DrawPartBounds) DrawPartBoundingBoxes(gl);

        if (Scene.ScanjobsPerHead == null) return;
        //DrawJobs(gl);
    }

    private static void DrawReference(GL gl, double length)
    {
        const double z = 0.0;

        // X
        gl.Color(1.0, 0.0, 0.0, 1.0);
        gl.LineWidth(1f);
        gl.Begin(PrimitiveType.LineStrip);
        gl.Vertex(0.0, 0.0, z);
        gl.Vertex(length, 0.0, z);
        gl.End();

        // Y
        gl.Color(0.0, 1.0, 0.0, 1.0);
        gl.LineWidth(1f);
        gl.Begin(PrimitiveType.LineStrip);
        gl.Vertex(0.0, 0.0, z);
        gl.Vertex(0.0, length, z);
        gl.End();

        // Z
        gl.Color(0.0, 0.0, 1.0, 1.0);
        gl.LineWidth(1f);
        gl.Begin(PrimitiveType.LineStrip);
        gl.Vertex(0.0, 0.0, z);
        gl.Vertex(0.0, 0.0, z + length);
        gl.End();
    }

    private void DrawPlatform(GL gl)
    {
        if (Scene.PrinterCharacteristics == null) return;

        // The platform including the overflow
        var actual = Scene.PrinterCharacteristics.ActualBuildVolume;
        DrawBuildVolume(gl, actual, dashedLine: false);

        // The platform without the overflow gap (a bit smaller)
        var nominal = Scene.PrinterCharacteristics.NominalBuildVolume;
        if (nominal != actual)
            DrawBuildVolume(gl, nominal, dashedLine: true);
    }

    private static void DrawBuildVolume(GL gl, BuildVolume buildVolume, bool dashedLine)
    {
        gl.Color(0f, 0f, 0f, 1f);
        gl.LineWidth(1f);

        if (dashedLine)
        {
            gl.PushAttrib(AttribMask.EnableBit);
            gl.LineStipple(1, 0xFF00); // See https://stackoverflow.com/questions/5321226/help-to-draw-a-dashed-line-in-opengl  
            gl.Enable(EnableCap.LineStipple);
        }

        gl.Begin(PrimitiveType.LineLoop);

        var radius = buildVolume.CornerRadius;
        const double z = 0.0;

        var xmin = -buildVolume.Width / 2.0;
        var xmax = buildVolume.Width / 2.0;
        var ymin = -buildVolume.Depth / 2.0;
        var ymax = buildVolume.Depth / 2.0;

        gl.Vertex(xmin + radius, ymin, z);
        gl.Vertex(xmax - radius, ymin, z);
        gl.DrawArc(xmax - radius, ymin + radius, radius, -Math.PI / 2, Math.PI / 2, z);

        gl.Vertex(xmax, ymin + radius, z);
        gl.Vertex(xmax, ymax - radius, z);
        gl.DrawArc(xmax - radius, ymax - radius, radius, 0.0, Math.PI / 2, z);

        gl.Vertex(xmax - radius, ymax, z);
        gl.Vertex(xmin + radius, ymax, z);
        gl.DrawArc(xmin + radius, ymax - radius, radius, Math.PI / 2, Math.PI / 2, z);

        gl.Vertex(xmin, ymax - radius, z);
        gl.Vertex(xmin, ymin + radius, z);
        gl.DrawArc(xmin + radius, ymin + radius, radius, Math.PI, Math.PI / 2, z);

        gl.End();
        if (dashedLine) gl.PopAttrib();
    }

    private void DrawHeads(GL gl)
    {
        if (Scene.PrinterCharacteristics == null) return;

        const double r = 10.0;
        const double z = 0.0;

        foreach (var head in Scene.PrinterCharacteristics.Heads)
        {
            var color = Palette.Get(head.ColorIndex, PaletteStyle.Light);
            gl.Color(color[0], color[1], color[2], 1.0);

            gl.LineWidth(2f);

            gl.PushMatrix();

            gl.Translate(head.Center.x, head.Center.y, z);
            gl.Rotate((float)head.Rotation, 0f, 0f, 1f); // Rotate around Z

            gl.Begin(PrimitiveType.Lines);

            // X axis
            gl.Vertex(0.0, 0.0, z);
            gl.Vertex(r, 0.0, z);
            // X arrow (draws the letter x)
            gl.Vertex(r, 2.0, z);
            gl.Vertex(r + 2.0, -2.0, z);
            gl.Vertex(r + 2.0, 2.0, z);
            gl.Vertex(r, -2.0, z);

            // Y axis
            gl.Vertex(0.0, 0.0, z);
            gl.Vertex(0.0, r, z);
            // Y arrow (draws the letter y)
            gl.Vertex(-1.0, r + 3.0, z);
            gl.Vertex(0.0, r + 1.0, z);
            gl.Vertex(1.0, r + 3.0, z);
            gl.Vertex(-1.0, r - 1.0, z);

            gl.End();
            gl.PopMatrix();
        }
    }

    private void DrawHeadFields(GL gl)
    {
        if (Scene.PrinterCharacteristics == null) return;

        const double z = 0.0;
        var headIndex = 0;
        var patterns = new ushort[]
        {
                0x8888, // 1000100010001000
                0x4444, // 0100010001000100 
                0x2222, // 0010001000100010
                0x1111 // 0001000100010001
        };

        foreach (var head in Scene.PrinterCharacteristics.Heads)
        {
            var pattern = patterns[headIndex++ % 4];

            var color = Palette.Get(head.ColorIndex, PaletteStyle.Light);
            gl.Color(color[0], color[1], color[2], 1.0);

            gl.PushMatrix();

            gl.Translate(head.Center.x, head.Center.y, z);
            gl.Rotate((float)head.Rotation, 0f, 0f, 1f); // Rotate around Z

            var fields = new[] { head.MaxField, head.TargetField };
            var isMaxField = true;
            foreach (var field in fields)
            {
                gl.LineWidth(isMaxField ? 2f : 4f);
                gl.PushAttrib(AttribMask.EnableBit);

                gl.LineStipple(1, pattern); // See https://stackoverflow.com/questions/5321226/help-to-draw-a-dashed-line-in-opengl  
                gl.Enable(EnableCap.LineStipple);

                gl.Begin(PrimitiveType.LineStrip);
                gl.Vertex(field.XMin, field.YMin, z);
                gl.Vertex(field.XMax, field.YMin, z);
                gl.Vertex(field.XMax, field.YMax, z);
                gl.Vertex(field.XMin, field.YMax, z);
                gl.Vertex(field.XMin, field.YMin, z);
                gl.End();

                gl.PopAttrib();
                isMaxField = false;
            }

            gl.PopMatrix();
        }
    }

    private void DrawPartBoundingBoxes(GL gl)
    {
        if (Scene.Project == null) return;

        const double z = 0.0;
        const double inflation = 1.0;

        foreach (var partInfo in Scene.Project.Parts)
        {
            gl.Color(0.5, 0.5, 0.5, 1.0);
            gl.LineWidth(2f);

            if (partInfo.Canceled)
            {
                gl.PushAttrib(AttribMask.EnableBit);
                gl.LineStipple(1, 0xFF00); // See https://stackoverflow.com/questions/5321226/help-to-draw-a-dashed-line-in-opengl  
                gl.Enable(EnableCap.LineStipple);
            }

            gl.Begin(PrimitiveType.LineLoop);
            gl.Vertex(partInfo.BoundingBox.Min.X - inflation, partInfo.BoundingBox.Min.Y - inflation, z);
            gl.Vertex(partInfo.BoundingBox.Min.X - inflation, partInfo.BoundingBox.Max.Y + inflation, z);
            gl.Vertex(partInfo.BoundingBox.Max.X + inflation, partInfo.BoundingBox.Max.Y + inflation, z);
            gl.Vertex(partInfo.BoundingBox.Max.X + inflation, partInfo.BoundingBox.Min.Y - inflation, z);
            gl.End();

            if (partInfo.Canceled) gl.PopAttrib();

            DrawPartId(gl, partInfo, z);
        }
    }

    private static void DrawPartId(GL gl, IPartInfo partInfo, double z)
    {
        var text = partInfo.Id.ToString();

        // Let's draw in the top left corner of the box
        const double cellSize = 4.0;
        var textHeight = cellSize;
        var textWidth = cellSize * text.Length;
        var bounds = new ScanBox(
            new ScanPoint(partInfo.BoundingBox.Min.X, partInfo.BoundingBox.Max.Y - textHeight),
            new ScanPoint(partInfo.BoundingBox.Min.X + textWidth, partInfo.BoundingBox.Max.Y));
        var generator = new ScanTextGenerator(bounds, new ScanProcessParameterSet(), false);
        var scanObject = generator.Generate(text);

        // Render
        foreach (var trajectory in scanObject.GetTransformedTraj(new ScanTransformation()))
        {
            if (trajectory is ScanIdle) continue;
            if (trajectory is ScanPointList) continue;

            var points = trajectory.GetPoints();

            gl.Begin(PrimitiveType.LineLoop);
            foreach (var point in points)
                gl.Vertex(point.X, point.Y, z);

            gl.End();
        }
    }
}
