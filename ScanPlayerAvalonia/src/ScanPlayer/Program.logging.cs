using System;
using System.Linq;
using AddUp.NCore;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace ScanPlayer
{
    partial class Program
    {
        public sealed class LogEventMemoryTarget : Target
        {
            private readonly string targetLogger;

            public LogEventMemoryTarget(string? logger = null) => targetLogger = logger ?? "";

            public event Action<LogEventInfo>? CapturedLog;

            protected override void Write(LogEventInfo logEvent)
            {
                if (string.IsNullOrEmpty(targetLogger) || logEvent.LoggerName == targetLogger)
                    CapturedLog?.Invoke(logEvent);
            }
        }

        static Program()
        {
            InMemoryLogTarget = new LogEventMemoryTarget();
            InMemoryPerfsLogTarget = new LogEventMemoryTarget(PerfsLoggerFactory.LoggerName);

            static void configureNLog()
            {
                var configuration = LogManager.Configuration ?? new LoggingConfiguration();
                configuration.AddRuleForAllLevels(InMemoryLogTarget);
                //configuration.AddRuleForAllLevels(InMemoryPerfsLogTarget);
                LogManager.Configuration = configuration;
                LogManager.ReconfigExistingLoggers();
            }

            LogManager.LogFactory.ConfigurationChanged += (s, e) =>
            {
                // NB: this can happen because of Common Logging.
                // Oddly enough this always happens in Release mode and never happens in Debug mode...
                if (e.ActivatedConfiguration != null && !e.ActivatedConfiguration.AllTargets.Any(t => t is LogEventMemoryTarget))
                    configureNLog();
            };

            configureNLog();
        }

        public static LogEventMemoryTarget InMemoryLogTarget { get; }
        public static LogEventMemoryTarget InMemoryPerfsLogTarget { get; }
    }
}
