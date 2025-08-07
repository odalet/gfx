using System;
using System.Collections.Generic;
using System.Linq;
using AddUp.NCore.Scan;
using AddUp.NCore.Scan.Geometry;
using AddUp.NCore.Scan.Trajectories;
using Avalonia;
using Avalonia.Skia;
using SkiaSharp;

namespace ScanPlayer.Rendering;

internal sealed class ScanTextGenerator
{
    public ScanTextGenerator(ScanBox bounds, ScanProcessParameterSet scanProcessParameters, bool drawBoundingBox = true)
    {
        Bounds = bounds ?? throw new ArgumentNullException(nameof(bounds));
        ScanProcessParameters = scanProcessParameters;
        DrawBoundingBox = drawBoundingBox;
    }

    private ScanBox Bounds { get; }
    private ScanProcessParameterSet ScanProcessParameters { get; }
    private bool DrawBoundingBox { get; }

    public ScanObject Generate(string text)
    {
        var scanObject = new ScanObject();

        // Bounding box...
        if (DrawBoundingBox)
        {
            var strip = new ScanLineStrip(ScanProcessParameters);
            strip.AddPoint(Bounds.XMin, Bounds.YMin);
            strip.AddPoint(Bounds.XMax, Bounds.YMin);
            strip.AddPoint(Bounds.XMax, Bounds.YMax);
            strip.AddPoint(Bounds.XMin, Bounds.YMax);
            strip.AddPoint(Bounds.XMin, Bounds.YMin); // And once again the 1st point

            // Let's check the trajectory is correct
            if (strip.GetNumberOfMoves() > 0)
                scanObject.AddTrajectory(strip);
        }

        var textTrajectories = GenerateTextTrajectories(text, new(Bounds.XMax - Bounds.XMin, Bounds.YMax - Bounds.YMin));
        foreach (var trajectory in textTrajectories.Where(t => t.GetNumberOfMoves() > 0))
            scanObject.AddTrajectory(trajectory);

        return scanObject;
    }

    private IEnumerable<ScanTrajectory> GenerateTextTrajectories(string text, Size constraint)
    {
        var textPaths = new List<(SKPath path, SKMatrix matrix)>();
        using (var typeface = SKTypeface.FromFamilyName("Arial", SKFontStyleWeight.Normal, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright))
        using (var font = new SKFont(typeface, 72f))
        using (var glyphTypefaceImpl = new GlyphTypefaceImpl(typeface))
        {
            var glyphs = new Span<ushort>(text.Select(c => glyphTypefaceImpl.GetGlyph(c)).ToArray());
            font.GetGlyphPaths(glyphs, (path, matrix) => textPaths.Add((path, matrix)));
        }

        return Array.Empty<ScanTrajectory>();

        //var typeface = new Typeface(new FontFamily("Arial"), FontStyle.Normal, FontWeight.Normal);
        //var formattedText = new FormattedText(text, typeface, 72.0, TextAlignment.Left, TextWrapping.NoWrap, constraint);

        ////formattedText.PlatformImpl

        //var lines = formattedText.GetLines().ToArray();
        //lines[0].

        //var geometry = formattedText.BuildGeometry(new Point(0.0, 0.0));
        //geometry.Transform = new ScaleTransform(1.0, -1.0);

        //return ComputePoints(geometry);
    }

    //private IEnumerable<ScanLineStrip> ComputePoints(M.Geometry geometry)
    //{
    //    var scaleX = geometry.Bounds.Width / Bounds.Width;
    //    var scaleY = geometry.Bounds.Height / Bounds.Height;
    //    var transX = Bounds.XMin;
    //    var transY = Bounds.YMin + (geometry.Bounds.Height - geometry.Bounds.Bottom) / scaleY;

    //    var figures = geometry.GetFlattenedPathGeometry().Figures;
    //    var polygons = figures.SelectMany(
    //        f => ComputePolygon(f, p => new ScanPoint(p.X / scaleX + transX, p.Y / scaleY + transY)));

    //    foreach (var polygon in polygons)
    //        yield return polygon;
    //}

    //private IEnumerable<ScanLineStrip> ComputePolygon(PathFigure figure, Func<Point, ScanPoint> transform)
    //{
    //    var currentPolygon = new ScanLineStrip(ScanProcessParameters);
    //    currentPolygon.AddPoint(transform(figure.StartPoint));

    //    foreach (var segment in figure.Segments)
    //        if (segment is M.PolyLineSegment polyLine)
    //        {
    //            foreach (var point in polyLine.Points)
    //                currentPolygon.AddPoint(transform(point));
    //        }

    //    yield return currentPolygon;
    //}
}