using System.Windows;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.Rendering;

namespace ScanPlayerWpf.Controls.Logging
{
    internal class LogLineStyle
    {
        public Brush BackgroundBrush { get; set; }
        public Brush ForegroundBrush { get; set; }
        public FontStyle? FontStyle { get; set; }
        public FontWeight? FontWeight { get; set; }

        public void ApplyTo(VisualLineElement element)
        {
            if (BackgroundBrush != null) element.TextRunProperties.SetForegroundBrush(BackgroundBrush);
            if (ForegroundBrush != null) element.TextRunProperties.SetForegroundBrush(ForegroundBrush);

            if (FontStyle != null || FontWeight != null)
            {
                var face = element.TextRunProperties.Typeface;
                element.TextRunProperties.SetTypeface(new Typeface(
                    face.FontFamily,
                    FontStyle ?? face.Style,
                    FontWeight ?? face.Weight,
                    face.Stretch
                ));
            }
        }
    }

}
