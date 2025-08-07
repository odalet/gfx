using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using AddUp.NCore.Scan.Geometry;

namespace ScanPlayer.Rendering;

// Provides a means to transform a simple segment into a 
// list of points that simulate wobbling
internal static class WobblingHelper
{
    public static IEnumerable<(double x, double y)> Transform(ScanPoint first, ScanPoint last, double thickness)
    {
        var norm = Norm(first, last);
        var segCount = (int)(norm / 0.1); // 0.1 mm = 100 µm
        if (segCount < 2)
        {
            yield return (first.X, first.Y);
            yield return (last.X, last.Y);
            yield break;
        }

        var points = SplitLine(first, last, segCount);

        var previous = points.First();
        yield return previous;

        var up = true;
        foreach (var p in points.Skip(1))
        {
            var r = Squigglize(previous, p, up, thickness);
            yield return r;

            previous = r;
            up = !up;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static (double x, double y) Squigglize((double x, double y) p1, (double x, double y) p2, bool up, double thickness)
    {
        // Translate to origin
        var dx = p2.x - p1.x;
        var dy = p2.y - p1.y;
        var norm = Math.Sqrt(dx * dx + dy * dy);

        // Rotate the vector (dx, dy) by +/- 90°
        var (x, y) = up ? (-dy, dx) : (dy, -dx);

        // Reduce its length
        x /= norm / thickness;
        y /= norm / thickness;

        // Add the new vector to p2
        return (p2.x + x, p2.y + y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static IEnumerable<(double x, double y)> SplitLine(ScanPoint p1, ScanPoint p2, int count) =>
        SplitLine((p1.X, p1.Y), (p2.X, p2.Y), count);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static IEnumerable<(double x, double y)> SplitLine((double x, double y) p1, (double x, double y) p2, int count)
    {
        yield return p1;

        for (var i = 1; i < count; i++)
        {
            var k = (double)i / count;
            yield return (p1.x + k * (p2.x - p1.x), p1.y + k * (p2.y - p1.y));
        }

        yield return p2;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static double Norm(ScanPoint p1, ScanPoint p2) => Norm(p1.X, p1.Y, p2.X, p2.Y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static double Norm(double x1, double y1, double x2, double y2) => Norm(x2 - x1, y2 - y1);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static double Norm(double x, double y) => Math.Sqrt(x * x + y * y);
}