using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiGpt.Data
{
    public static class SimpleMemLogger
    {
        private static IList<string> _lines = new List<string>();

        public static void Log(string message)
        {
            _lines.Add(message);
        }

        public static void Reset()
        {
            _lines = new List<string>();
        }

        public new static string ToString(int maxLineLength = 0)
        {
            IList<string> newLines = new List<string>();
            foreach (var line in _lines)
            {
                if (maxLineLength == 0 || line.Length <= maxLineLength)
                {
                    newLines.Add(line + "  ");
                }
                else
                {
                    newLines.Add(line.Substring(0, maxLineLength) + "..  ");
                }
            }

            return string.Join(Environment.NewLine, newLines);
        }
    }
}
