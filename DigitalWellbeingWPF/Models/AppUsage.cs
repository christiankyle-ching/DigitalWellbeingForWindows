using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DigitalWellbeingWPF.Models
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
