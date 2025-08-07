using System;
using Avalonia.Input;

namespace ScanPlayer
{
    internal sealed class CursorScope : IDisposable
    {
        private readonly InputElement control;
        private readonly Cursor? previousCursor;
        private readonly Cursor newCursor;

        public CursorScope(InputElement owner, StandardCursorType cursorType)
        {
            control = owner;
            previousCursor = control.Cursor;
            newCursor = new Cursor(cursorType);
            control.Cursor = newCursor;
        }

        public void Dispose()
        {
            newCursor.Dispose();
            control.Cursor = previousCursor;
        }
    }
}
