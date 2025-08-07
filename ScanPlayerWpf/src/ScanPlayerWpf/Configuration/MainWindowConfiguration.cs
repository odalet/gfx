namespace ScanPlayerWpf.Configuration
{
    internal readonly struct MainWindowConfiguration
    {
        public MainWindowConfiguration(double x, double y, double width, double height, bool maximized)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            IsMaximized = maximized;
        }

        public static MainWindowConfiguration Default { get; } = 
            new MainWindowConfiguration(100.0, 100.0, 640.0, 480.0, false);

        public double X { get; }
        public double Y { get; }
        public double Width { get; }
        public double Height { get; }
        public bool IsMaximized { get; }
    }
}
