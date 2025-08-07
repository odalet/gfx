using System;
using System.Windows.Input;

namespace MvvmLightTests.Controls
{
    public static class Wpf
    {
        // Copied from https://stackoverflow.com/questions/3480966/display-hourglass-when-application-is-busy
        private class WaitCursor : IDisposable
        {
            private readonly Cursor savedCursor;

            public WaitCursor()
            {
                savedCursor = Mouse.OverrideCursor;
                Mouse.OverrideCursor = Cursors.Wait;
            }
            
            public void Dispose() => Mouse.OverrideCursor = savedCursor;
        }

        public static IDisposable ShowWaitCursor() => new WaitCursor();
    }
}
