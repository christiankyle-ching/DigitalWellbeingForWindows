using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalWellbeingWPF.Helpers
{
    public static class StringHelper
    {
        public static string NEWLINE = Environment.NewLine;

        public static string TimeSpanToString(TimeSpan duration)
        {
            string durationStr = (int)duration.Hours > 0 ? $"{duration.Hours}h " : "";
            durationStr += (int)duration.TotalMinutes > 0 ? $"{duration.Minutes}m " : "";
            durationStr += (int)duration.TotalSeconds > 0 ? $"{duration.Seconds}s " : "";

            return durationStr.Trim();
        }

        private static readonly TextInfo txtInfo = new CultureInfo("en-US", false).TextInfo;
        public static string TitleCaseWhenLower(string processName)
        {
            return processName.Any(char.IsUpper) ? processName : txtInfo.ToTitleCase(processName);
        }

        public static string TimeSpanToShortString(TimeSpan duration)
        {
            return $"{duration.Hours}h {duration.Minutes}m";
        }

        public static string ShortenBytes(ulong bytes)
        {
            ulong MB = bytes / 1048576;
            ulong GB = MB / 1024;

            if (GB > 0)
            {
                return $"{GB} GB";
            }
            else
            {
                return $"{MB} MB";
            }
        }

    }
}
