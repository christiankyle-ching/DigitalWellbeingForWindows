using System;

namespace DigitalWellbeingUI.Helpers
{
    public static class StringParser
    {
        public static string TimeSpanToString(TimeSpan duration)
        {
            string durationStr = (int)duration.Hours > 0 ? $"{duration.Hours} hour/s " : "";
            durationStr += (int)duration.TotalMinutes > 0 ? $"{duration.Minutes} minute/s " : "";
            durationStr += (int)duration.TotalSeconds > 0 ? $"{duration.Seconds} second/s " : "";

            return durationStr.Trim();
        }
    }
}
