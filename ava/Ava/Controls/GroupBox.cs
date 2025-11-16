using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace Ava.Controls;

// Copied from https://github.com/wieslawsoltes/GroupBox.Avalonia/blob/main/GroupBox.Avalonia

public class GroupBox : HeaderedContentControl { }

internal sealed class GroupBoxClipConverter : IMultiValueConverter
{
    private static readonly GroupBoxClipConverter instance = new();

    public object Convert(IList<object?>? values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values is null || values.Count != 2 || values[0] is not Rect bounds || values[1] is not Rect gap)
            return new BindingNotification(
                new ArgumentException("Expecting two non-empty rectangles (type Avalonia.Rect)."),
                BindingErrorType.Error);

        gap = bounds.Intersect(gap);

        return new CombinedGeometry(
            GeometryCombineMode.Exclude,
            new RectangleGeometry { Rect = new(bounds.Size) },
            new RectangleGeometry { Rect = new(gap.Position - bounds.Position, gap.Size) });
    }

    public IMultiValueConverter ProvideValue(IServiceProvider serviceProvider) => instance;
}