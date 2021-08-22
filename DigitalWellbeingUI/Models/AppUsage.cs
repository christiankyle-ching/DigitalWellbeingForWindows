using System;

namespace DigitalWellbeingUI.Models
{
    public class AppUsage
    {
        public string Name { get; set; }
        public TimeSpan Duration { get; set; }

        public AppUsage(string appName, TimeSpan duration)
        {
            this.Name = appName;
            this.Duration = duration;
        }
    }
}
