using System;

namespace ScanPlayer.Rendering;

internal record Trackball
{
    public Trackball() => Reset();

    public float Radius { get; set; }
    public float Theta { get; set; } // Rotates around z
    public float Phi { get; set; } // Rotates around x
    public float X { get; set; }
    public float Y { get; set; }

    public void Reset()
    {
        Radius = 0.8f;
        Theta = (float)Math.PI;
        Phi = 0f;
        X = 0f;
        Y = 0f;
    }
}