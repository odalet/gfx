using System;
using System.Linq;
using SharpDX;

namespace ScanPlayerWpf.Rendering
{
    internal enum PaletteStyle
    {
        Normal,
        Dark,
        Light
    }

    internal static class Palette
    {
        // Built from http://www.creativecolorschemes.com/resources/free-color-schemes/basic-color-scheme.shtml

        private static readonly (byte r, byte g, byte b)[] dark = new[]
        {
            (178, 31, 53), // red
            (0, 117, 58), // green
            (0, 82, 165),  // blue
            (255, 161, 53), // orange
            (104, 30, 126), // purple

        }.Select(x => ((byte)x.Item1, (byte)x.Item2, (byte)x.Item3)).ToArray();

        private static readonly (byte r, byte g, byte b)[] normal = new[]
        {
            (216, 39, 53), // red
            (0, 158, 71), // green
            (0, 121, 231),  // blue
            (255, 203, 53), // orange
            (125, 60, 181), // purple

        }.Select(x => ((byte)x.Item1, (byte)x.Item2, (byte)x.Item3)).ToArray();

        private static readonly (byte r, byte g, byte b)[] light = new[]
        {
            (255, 116, 53), // red
            (22, 221, 53), // green
            (0, 169, 252),  // blue
            (255, 240, 53), // orange
            (189, 122, 246), // purple

        }.Select(x => ((byte)x.Item1, (byte)x.Item2, (byte)x.Item3)).ToArray();

        public static Color4 GetColor4(int index, PaletteStyle style = PaletteStyle.Normal) => GetColor4(index, 1f, style);
        public static Color4 GetColor4(int index, float alpha, PaletteStyle style = PaletteStyle.Normal)
        {
            var rgb = Normalize(GetRgbBytes(index, style));
            return new Color4(rgb[0], rgb[1], rgb[2], alpha);
        }

        public static (byte r, byte g, byte b) GetRgbBytes(int index, PaletteStyle style)
        {
            var palette = normal;
            switch (style)
            {
                case PaletteStyle.Dark: palette = dark; break;
                case PaletteStyle.Light: palette = light; break;
            }

            if (index < palette.Length)
                return palette[index];

            var rnd = new Random(index);
            return (
                (byte)(255.0 * rnd.NextDouble()),
                (byte)(255.0 * rnd.NextDouble()),
                (byte)(255.0 * rnd.NextDouble()));
        }

        private static float[] Normalize((byte r, byte g, byte b) color) => new[]
        {
            color.r / 255f, color.g / 255f, color.b / 255f,
        };
    }
}
