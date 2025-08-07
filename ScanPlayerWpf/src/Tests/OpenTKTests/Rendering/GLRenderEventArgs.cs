using System;
using System.Windows;

namespace OpenTKTests.Rendering
{
    public sealed class GlRenderEventArgs : EventArgs
    {
        public GlRenderEventArgs(int width, int height, bool resized, bool screenshot, bool newContext)
        {
            Width = width;
            Height = height;
            RepaintRect = new Int32Rect(0, 0, Width, Height);
            Resized = resized;
            Screenshot = screenshot;
            NewContext = newContext;
        }

        public bool Resized { get; }
        public bool Screenshot { get; }
        public bool NewContext { get; }
        public int Width { get; }
        public int Height { get; }
        public Int32Rect RepaintRect { get; set; }
    }
}
