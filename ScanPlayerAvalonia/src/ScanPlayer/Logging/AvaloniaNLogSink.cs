using System;
using System.Collections.Generic;
using System.Text;
using Avalonia.Controls;
using Avalonia.Logging;
using Avalonia.Utilities;
using N = NLog;

namespace ScanPlayer.Logging
{
    internal static class AvaloniaNLogSinkExtensions
    {
        public static T LogToNLog<T>(this T builder, LogEventLevel level = LogEventLevel.Verbose, params string[] areas) where T : AppBuilderBase<T>, new()
        {
            Logger.Sink = new AvaloniaNLogSink(level, areas);
            return builder;
        }
    }

    internal sealed class AvaloniaNLogSink : ILogSink
    {
        private readonly LogEventLevel minimumLevel;
        private readonly IList<string>? areas;
        private static readonly Dictionary<string, N.ILogger> loggers = new();

        // no areas to log means log everything
        public AvaloniaNLogSink(LogEventLevel minLevel, IList<string>? areasToLog = null)
        {
            minimumLevel = minLevel;
            areas = (areasToLog != null && areasToLog.Count > 0) ? areasToLog : null;
        }

        public bool IsEnabled(LogEventLevel level, string area) =>
            level >= minimumLevel && (areas?.Contains(area) ?? true);

        public void Log(LogEventLevel level, string area, object? source, string messageTemplate)
        {
            if (IsEnabled(level, area))
                Log(level, area, Format<object, object, object>(messageTemplate, source));
        }

        public void Log<T0>(LogEventLevel level, string area, object? source, string messageTemplate, T0 propertyValue0)
        {
            if (IsEnabled(level, area))
                Log(level, area, Format<T0, object, object>(messageTemplate, source, propertyValue0));
        }

        public void Log<T0, T1>(LogEventLevel level, string area, object? source, string messageTemplate, T0 propertyValue0, T1 propertyValue1)
        {
            if (IsEnabled(level, area))
                Log(level, area, Format<T0, T1, object>(messageTemplate, source, propertyValue0, propertyValue1));
        }

        public void Log<T0, T1, T2>(LogEventLevel level, string area, object? source, string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2)
        {
            if (IsEnabled(level, area))
                Log(level, area, Format<T0, T1, T2>(messageTemplate, source, propertyValue0, propertyValue1, propertyValue2));
        }

        public void Log(LogEventLevel level, string area, object? source, string messageTemplate, params object?[] propertyValues)
        {
            if (IsEnabled(level, area))
                Log(level, area, Format(messageTemplate, source, propertyValues));
        }

        private static void Log(LogEventLevel level, string area, string message)
        {
            static N.LogLevel translate(LogEventLevel l) => l switch
            {
                LogEventLevel.Verbose => N.LogLevel.Trace,
                LogEventLevel.Debug => N.LogLevel.Debug,
                LogEventLevel.Warning => N.LogLevel.Warn,
                LogEventLevel.Information => N.LogLevel.Info,
                LogEventLevel.Error => N.LogLevel.Error,
                LogEventLevel.Fatal => N.LogLevel.Fatal,
                _ => N.LogLevel.Trace
            };

            static N.ILogger getLogger(string area)
            {
                var loggerName = $"Avalonia ({area})";
                if (!loggers.ContainsKey(loggerName))
                    loggers.Add(loggerName, N.LogManager.GetLogger(loggerName));
                return loggers[loggerName];
            }

            var nlogLevel = translate(level);
            var logger = getLogger(area);
            logger.Log(nlogLevel, message);
        }

        private static string Format<T0, T1, T2>(string template, object? source, T0? v0 = default, T1? v1 = default, T2? v2 = default)
        {
            var characterReader = new CharacterReader(MemoryExtensions.AsSpan(template));
            var builder = new StringBuilder(template.Length);
            var num = 0;

            while (!characterReader.End)
            {
                char c = characterReader.Take();
                if (c != '{')
                    _ = builder.Append(c);
                else if (characterReader.Peek != '{')
                {
                    _ = builder
                        .Append('\'')
                        .Append(num++ switch
                        {
                            0 => v0,
                            1 => v1,
                            2 => v2,
                            _ => null,
                        })
                        .Append('\'');

                    _ = characterReader.TakeUntil('}');
                    _ = characterReader.Take();
                }
                else
                {
                    _ = builder.Append('{');
                    _ = characterReader.Take();
                }
            }

            if (source != null) _ = builder
                    .Append(" (")
                    .Append(source.GetType().Name)
                    .Append(" #")
                    .Append(source.GetHashCode())
                    .Append(')');

            return builder.ToString();
        }

        private static string Format(string template, object? source, object?[] v)
        {
            var characterReader = new CharacterReader(MemoryExtensions.AsSpan(template));
            var builder = new StringBuilder(template.Length);
            var num = 0;

            while (!characterReader.End)
            {
                char c = characterReader.Take();
                if (c != '{')
                    _ = builder.Append(c);
                else if (characterReader.Peek != '{')
                {
                    _ = builder
                        .Append('\'')
                        .Append((num < v.Length) ? v[num++] : null)
                        .Append('\'');

                    _ = characterReader.TakeUntil('}');
                    _ = characterReader.Take();
                }
                else
                {
                    _ = builder.Append('{');
                    _ = characterReader.Take();
                }
            }

            if (source != null) _ = builder
                    .Append(" (")
                    .Append(source.GetType().Name)
                    .Append(" #")
                    .Append(source.GetHashCode())
                    .Append(')');

            return builder.ToString();
        }
    }
}
