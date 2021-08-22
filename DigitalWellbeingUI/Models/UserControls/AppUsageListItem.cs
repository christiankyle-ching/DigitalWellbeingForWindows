using System;
using System.Windows.Media;
using DigitalWellbeingUI.Helpers;

namespace DigitalWellbeingUI.Models.UserControls
{
    public class AppUsageListItem
    {
        public int Percentage { get; set; }

        public string AppName { get; set; }
        public TimeSpan Duration { get; set; }
        public string StrDuration { get => StringParser.TimeSpanToString(Duration); }
        public ImageSource IconSource { get; set; }

        public AppUsageListItem(string appName, TimeSpan duration, int percentage)
        {
            AppName = appName;
            Duration = duration;
            Percentage = percentage;
            IconSource = IconManager.GetIconSource(appName);
        }

    }
}
