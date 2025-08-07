using System;
using System.Linq;

namespace ScanPlayer.Rendering;

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

    ////public static Color GetColor(int index, PaletteStyle style = PaletteStyle.Normal)
    ////{
    ////    var (r, g, b) = GetRgbBytes(index, style);
    ////    return Color.FromArgb(r, g, b);
    ////}

    public static double[] Get(int index, PaletteStyle style = PaletteStyle.Normal) =>
        Normalize(GetRgbBytes(index, style));

    private static (byte r, byte g, byte b) GetRgbBytes(int index, PaletteStyle style)
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

    private static double[] Normalize((byte r, byte g, byte b) color) => new[]
    {
        color.r / 255.0, color.g / 255.0, color.b / 255.0,
    };
}