//using System;
//using System.Threading;
//using AddUp.NCore.Logging; // from AddUp.AnyLog

//namespace AddUp.NCore
//{
//    // NB: this is a general purpose utility class and would rather exist in AddUp.NCore.Utils...
//    // However, doing so would add a new dependency to job services. This is why, at least for now,
//    // it exists only in AddUp.NCore.Printing.Common.dll (but being public, it can be used by anything
//    // depending on it)
//    public interface IPerfsLogger
//    {
//        void Measure(Action action, string operation);
//        T Measure<T>(Func<T> func, string operation);
//        IDisposable Measure(string operation);
//        void Log(string text);
//    }

//    public static class PerfsLoggerFactory
//    {
//        private sealed class NopPerfsLogger : IPerfsLogger
//        {
//            private sealed class Disposable : IDisposable
//            {
//                public void Dispose() { /* Empty by design */ }
//            }

//            private static readonly IDisposable disposable = new Disposable();

//            public void Log(string text) { /* Empty by design */ }
//            public void Measure(Action action, string operation) => action();
//            public T Measure<T>(Func<T> func, string operation) => func();
//            public IDisposable Measure(string operation) => disposable;
//        }

//        public static IPerfsLogger NopLogger { get; } = new NopPerfsLogger();
//        public static string LoggerName => "PERFS";

//        public static IPerfsLogger MakeTrace() => new PerfsLogger(LoggerName, LogLevel.Trace);
//    }

//    public sealed class PerfsLogger : IPerfsLogger
//    {
//        private sealed class Operation : IDisposable
//        {
//            private readonly PerfsLogger perfLogger;
//            private readonly string name;
//            private readonly DateTimeOffset startDate;

//            public Operation(PerfsLogger owner, string operationName, DateTimeOffset when)
//            {
//                perfLogger = owner;
//                name = operationName;
//                startDate = when;
//                perfLogger.Log($"{" ",12}|{GetCurrentOperationPrefix(true)}{name}");
//                _ = Interlocked.Increment(ref nestingLevel);
//            }

//            public void Dispose()
//            {
//                _ = Interlocked.Decrement(ref nestingLevel);
//                var elapsed = DateTimeOffset.Now - startDate;
//                perfLogger.Log($"{FormatDuration(elapsed)}|{GetCurrentOperationPrefix(false)}{name}");
//            }

//            private static string GetCurrentOperationPrefix(bool enter)
//            {
//                var prefix = enter ? enterPrefix : exitPrefix;
//                var blanks = new string(' ', nestingLevel);
//                return blanks + prefix;
//            }

//            private static string FormatDuration(TimeSpan duration) => duration.ToString(@"hh\:mm\:ss\.fff").PadLeft(12);
//        }

//        private const string enterPrefix = "> ";
//        private const string exitPrefix = "< ";

//        private static int nestingLevel;
//        private readonly ILog log;
//        private readonly LogLevel level;

//        internal PerfsLogger(string loggerName, LogLevel logLevel)
//        {
//            log = LogManager.GetLogger(loggerName);
//            level = logLevel;
//        }

//        public void Measure(Action action, string operation)
//        {
//            using (Measure(operation))
//                action();
//        }

//        public T Measure<T>(Func<T> func, string operation)
//        {
//            T result;
//            using (Measure(operation))
//                result = func();
//            return result;
//        }

//        public IDisposable Measure(string operation) => new Operation(this, operation ?? "", DateTimeOffset.Now);

//        public void Log(string text) => log.Log(level, text ?? "");
//    }
//}
