using System;
using System.Collections.Generic;

namespace ScanPlayerWpf.Rendering
{
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
    }
}
