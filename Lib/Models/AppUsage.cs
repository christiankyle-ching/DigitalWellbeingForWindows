using System;

namespace Lib
{
    public class AppUsage
    {
        public string appName { get; set; }
        public TimeSpan usageDuration { get; set; }

        public AppUsage(string appName, TimeSpan duration)
        {
            this.appName = appName;
            this.usageDuration = duration;
        }
    }
}
