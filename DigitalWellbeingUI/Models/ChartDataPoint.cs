using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DigitalWellbeingUI.Models
{
    public class ChartDataPoint : INotifyPropertyChanged
    {
        public string Name { get; set; }
        public int Value { get; set; }

        public ChartDataPoint(string name, int value)
        {
            this.Name = name;
            this.Value = value;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Refresh()
        {
            OnPropertyChanged(nameof(Name));
            OnPropertyChanged(nameof(Value));
        }
    }
}
