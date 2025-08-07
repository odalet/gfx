using System;
using System.Collections.Generic;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;
using NLog;
using System.Linq;

namespace ScanPlayerWpf.Controls.Logging
{
    internal class LogColorizer : DocumentColorizingTransformer
    {
        private readonly List<int> startOffsets = new List<int>();
        private readonly Dictionary<int, LogLineInfo> dictionary = new Dictionary<int, LogLineInfo>();
        private readonly Func<LogLevel, LogLineStyle> getStyle;

        public LogColorizer(Func<LogLevel, LogLineStyle> getLogLineStyle) => getStyle = getLogLineStyle;

        public void Clear()
        {
            startOffsets.Clear();
            dictionary.Clear();
        }

        public void ClearOldData(int nbrOfLineToDelete)
        {
            startOffsets.RemoveRange(0, nbrOfLineToDelete);

            // Ugly, but it works...
            var test = 0;
            var keys = dictionary.Keys.Cast<int>().ToList();
            foreach(var key in keys)
            {
                _ = dictionary.Remove(key);
                test++;
                if (test == nbrOfLineToDelete)
                    break;
            }
        }

        public void AddLogLineInfo(LogLineInfo info)
        {
            if (dictionary.ContainsKey(info.StartOffset))
                dictionary[info.StartOffset] = info;
            else
            {
                startOffsets.Add(info.StartOffset);
                dictionary.Add(info.StartOffset, info);
            }
        }

        /// <summary>
        /// Override this method to colorize an individual document line.
        /// </summary>
        /// <param name="line"></param>
        protected override void ColorizeLine(DocumentLine line)
        {
            if (line == null || line.Length == 0) return;

            var info = FindLineInfo(line);
            if (info == null) return;

            var start = line.Offset > info.StartOffset ? line.Offset : info.StartOffset;
            var end = info.EndOffset > line.EndOffset ? line.EndOffset : info.EndOffset;

            LogLineStyle style = null;
            if (getStyle != null) style = getStyle(info.Level);
            if (style != null)
                ChangeLinePart(start, end, element => style.ApplyTo(element));
        }

        private LogLineInfo FindLineInfo(DocumentLine line)
        {
            var offset = FindNearestOffset(line.Offset);
            return offset.HasValue && dictionary.ContainsKey(offset.Value) ? dictionary[offset.Value] : null;
        }

        private int? FindNearestOffset(int offset)
        {
            var index = startOffsets.FindLastIndex(o => o <= offset);
            return index == -1 ? null : (int?)startOffsets[index];
        }
    }
}
