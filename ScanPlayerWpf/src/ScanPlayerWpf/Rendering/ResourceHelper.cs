using System.IO;
using Cyotek.Drawing.BitmapFont;
using HelixToolkit.Wpf.SharpDX;

namespace ScanPlayerWpf.Rendering
{
    public static class ResourceHelper
    {
        private static BitmapFont bitmapFont;
        private static BitmapFont BitmapFont => bitmapFont ?? (bitmapFont = LoadBitmapFont());

        private static byte[] fontTexture;
        private static byte[] FontTexture => fontTexture ?? (fontTexture = File.ReadAllBytes(@"Resources\SegoeScript.dds"));

        private static byte[] dotTexture;
        private static byte[] DotTexture => dotTexture ?? (dotTexture = File.ReadAllBytes(@"Resources\dot.png"));

        public static BillboardText3D Create() => new BillboardText3D(BitmapFont, new MemoryStream(FontTexture));

        public static TextureModel CreateDotTexture() => new MemoryStream(DotTexture);

        private static BitmapFont LoadBitmapFont()
        {
            var font = new BitmapFont();
            font.Load(@"Resources\SegoeScript.fnt");
            return font;
        }
    }
}
