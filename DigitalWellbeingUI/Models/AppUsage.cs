using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DigitalWellbeingUI.Models
{
    public class AppUsage : INotifyPropertyChanged
    {
        public string Name { get; set; }
        public TimeSpan Duration { get; set; }

        public AppUsage(string appName, TimeSpan duration)
        {
            this.Name = appName;
            this.Duration = duration;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Refresh()
        {
            OnPropertyChanged(nameof(Name));
            OnPropertyChanged(nameof(Duration));
        }
    }
}
