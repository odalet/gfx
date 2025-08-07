using OpenTK;

namespace OpenTKTests.Rendering
{
    public static class RenderingEngine
    {
        private static Toolkit toolkit;

        public static void Initialize() => toolkit = Toolkit.Init(new ToolkitOptions
        {
            Backend = PlatformBackend.PreferNative
        });

        public static void Uninitalize()
        {
            toolkit?.Dispose();
            toolkit = null;
        }
    }
}
