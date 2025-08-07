using NLog;

namespace ScanPlayerWpf.Controls.Logging
{
    internal class LogLineInfo
    {
        public int StartOffset { get; set; }
        public int EndOffset { get; set; }
        public LogLevel Level { get; set; }
    }
}
