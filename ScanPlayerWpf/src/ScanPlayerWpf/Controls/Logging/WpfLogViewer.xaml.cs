using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using GalaSoft.MvvmLight.Command;
using ICSharpCode.AvalonEdit.Highlighting;
using NLog;
using NLog.Layouts;
using CL = Common.Logging;

namespace ScanPlayerWpf.Controls.Logging
{
    /// <summary>
    /// Interaction logic for WpfLogViewer.xaml
    /// </summary>
    public partial class WpfLogViewer : UserControl
    {
        private const string formatString = "${longdate}|${pad:padding=-5:inner=${level:uppercase=true}}|${pad:padding=-24:fixedLength=True:alignmentOnTruncation=right:inner=${threadname}}[${pad:padding=3:fixedLength=True:inner=${threadid}}]|${pad:padding=-40:fixedLength=True:alignmentOnTruncation=right:inner=${logger}}|${message}${onexception:inner=${newline}${exception:format=tostring}}";
        private const int maxLineDisplayed = 100000;
        private const int nbrOfLineToDeleteWhenLimitIsReached = 50000;

        private static readonly CL.ILog log = CL.LogManager.GetLogger(typeof(WpfLogViewer));
        private static readonly SimpleLayout layout = new SimpleLayout(formatString);
        private static int counter;

        private readonly LogEventMemoryTarget logTarget;
        private readonly LogColorizer colorizer;
        private LogLevel thresholdLogLevel = LogLevel.Trace;

        public WpfLogViewer()
        {
            InitializeComponent();

            DebugBar.Visibility = Visibility.Collapsed;

            colorizer = CreateColorizer();
            logBox.TextArea.TextView.LineTransformers.Add(colorizer);

            logTarget = new LogEventMemoryTarget();
            logTarget.EventReceived += info => DispatchLog(info);

            var config = LogManager.Configuration;

            config.AddTarget("wpfLogViewer", logTarget);
            config.AddRule(LogLevel.Trace, LogLevel.Fatal, "wpfLogViewer");

            LogManager.Configuration = config;

            foreach (var level in LogLevel.AllLevels.OrderByDescending(l => l.Ordinal))
                _ = LevelBox.Items.Add(level);

            LevelBox.SelectedItem = thresholdLogLevel;
            LevelBox.SelectionChanged += (s, _) => thresholdLogLevel = SelectedLogLevel;

            ClearCommand = new RelayCommand(() =>
            {
                if (colorizer != null) colorizer.Clear();
                logBox.Document.Text = string.Empty;
            }, () => !string.IsNullOrEmpty(logBox.Document.Text));

            CopyAllCommand = new RelayCommand(() =>
            {
                try
                {
                    var text = logBox.Document.Text;
                    var data = new DataObject(text);
                    var html = HtmlClipboard.CreateHtmlFragment(
                        logBox.Document, null, null, new HtmlOptions(logBox.Options));
                    HtmlClipboard.SetHtml(data, html);
                    Clipboard.SetDataObject(data, true);
                }
                catch (Exception ex)
                {
                    // There was a problem while writing to the clipboard... let's log it!
                    log.Error(ex);
                }
            }, () => !string.IsNullOrEmpty(logBox.Document.Text));
        }

        public ICommand ClearCommand { get; }
        public ICommand CopyAllCommand { get; }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose() => logTarget.Dispose();

        /// <summary>
        /// Gets or sets the selected log level.
        /// </summary>
        /// <value>The selected log level.</value>
        public LogLevel SelectedLogLevel => (LogLevel)LevelBox.SelectedItem;

        private void DispatchLog(LogEventInfo entry)
        {
            if (!Dispatcher.CheckAccess())
                _ = Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action<LogEventInfo>(e => LogEntryToTextBox(e)), entry);
            else LogEntryToTextBox(entry);
        }

        private void LogEntryToTextBox(LogEventInfo entry)
        {
            if (thresholdLogLevel.Ordinal > entry.Level.Ordinal)
                return;

            var start = logBox.Document.TextLength;
            logBox.AppendText(layout.Render(entry));
            logBox.AppendText("\r");
            var end = logBox.Document.TextLength;
            colorizer.AddLogLineInfo(new LogLineInfo
            {
                StartOffset = start,
                EndOffset = end,
                Level = entry.Level
            });

            if (logBox.Document.LineCount > maxLineDisplayed)
            {
                // Delete old lines in textbox
                var line = logBox.Document.GetLineByNumber(nbrOfLineToDeleteWhenLimitIsReached);
                var endOffset = line.EndOffset + line.DelimiterLength;
                logBox.Document.Remove(0, endOffset);

                // Delete old data in colorizer
                if (colorizer != null)
                    colorizer.ClearOldData(nbrOfLineToDeleteWhenLimitIsReached);
            }

            logBox.ScrollToEnd();
        }

        private static LogColorizer CreateColorizer()
        {
            var logLinesStyle = new Dictionary<LogLevel, LogLineStyle>
            {
                [LogLevel.Fatal] = new LogLineStyle
                {
                    ForegroundBrush = Brushes.Red,
                    FontWeight = FontWeights.Bold
                },
                [LogLevel.Error] = new LogLineStyle
                {
                    ForegroundBrush = Brushes.Red
                },
                [LogLevel.Warn] = new LogLineStyle
                {
                    ForegroundBrush = Brushes.Orange,
                    FontWeight = FontWeights.Bold
                },
                [LogLevel.Info] = new LogLineStyle
                {
                    FontWeight = FontWeights.Bold
                },
                [LogLevel.Debug] = new LogLineStyle
                {
                    ForegroundBrush = Brushes.Blue
                },
                [LogLevel.Trace] = new LogLineStyle()
            };

            return new LogColorizer(ll => logLinesStyle.ContainsKey(ll) ? logLinesStyle[ll] : null);
        }

        [SuppressMessage("Critical Code Smell", "S2696:Instance members should not write to \"static\" fields", Justification = "")]
        private void FatalButton_Click(object sender, RoutedEventArgs e)
        {
            counter++;
            log.Fatal($"Fatal #{counter}");
        }

        [SuppressMessage("Critical Code Smell", "S2696:Instance members should not write to \"static\" fields", Justification = "")]
        private void ErrorButton_Click(object sender, RoutedEventArgs e)
        {
            counter++;
            log.Error($"Error #{counter}");
        }

        [SuppressMessage("Critical Code Smell", "S2696:Instance members should not write to \"static\" fields", Justification = "")]
        private void WarningButton_Click(object sender, RoutedEventArgs e)
        {
            counter++;
            log.Warn($"Warning #{counter}");
        }

        [SuppressMessage("Critical Code Smell", "S2696:Instance members should not write to \"static\" fields", Justification = "")]
        private void InfoButton_Click(object sender, RoutedEventArgs e)
        {
            counter++;
            log.Info($"Info #{counter}");
        }

        [SuppressMessage("Critical Code Smell", "S2696:Instance members should not write to \"static\" fields", Justification = "")]
        private void DebugButton_Click(object sender, RoutedEventArgs e)
        {
            counter++;
            log.Debug($"Debug #{counter}");
        }

        [SuppressMessage("Critical Code Smell", "S2696:Instance members should not write to \"static\" fields", Justification = "")]
        private void TraceButton_Click(object sender, RoutedEventArgs e)
        {
            counter++;
            log.Trace($"Trace #{counter}");
        }
    }
}
