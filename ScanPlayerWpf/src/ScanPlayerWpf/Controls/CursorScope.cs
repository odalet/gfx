using System;
using System.Windows;
using System.Windows.Input;

namespace ScanPlayerWpf.Controls
{
    internal sealed class CursorScope : IDisposable
    {
        private readonly FrameworkElement element;
        private readonly Cursor previousCursor;

        public CursorScope(FrameworkElement owner, Cursor cursor)
        {
            element = owner;
            previousCursor = element.Cursor;
            element.Cursor = cursor;
        }

        public void Dispose() => element.Cursor = previousCursor;
    }
}
