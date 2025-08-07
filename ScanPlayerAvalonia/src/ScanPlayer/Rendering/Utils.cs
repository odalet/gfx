using System;
using System.Collections.Generic;
using ScanPlayer.OpenGL;

namespace ScanPlayer.Rendering;

internal static class Utils
{
    public static IEnumerable<(double x, double y, double z)> GetArc(double ox, double oy, double r, double startAngle, double arcAngle, int segmentsCount, double z)
    {
        var theta = arcAngle / segmentsCount;
        var angle = startAngle;
        for (var ii = 0; ii < segmentsCount; ii++)
        {
            var dx = r * Math.Cos(angle);
            var dy = r * Math.Sin(angle);

            yield return (ox + dx, oy + dy, z);
            angle += theta;
        }
    }

    public static void DrawArc(this GL gl, double ox, double oy, double r, double startAngle, double arcAngle, double z)
    {
        foreach (var point in GetArc(ox, oy, r, startAngle, arcAngle, 8, z))
            gl.Vertex(point.x, point.y, point.z);
    }
}