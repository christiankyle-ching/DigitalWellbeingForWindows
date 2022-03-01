using DigitalWellbeingWPF.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace DigitalWellbeingWPF.Models.UserControls
{
    public class AppUsageListItem : INotifyPropertyChanged
    {
        public int Percentage { get; set; }

        public string ProcessName { get; set; }
        public string ProgramName { get; set; }
        public TimeSpan Duration { get; set; }
        public string StrDuration { get => StringHelper.TimeSpanToString(Duration); }
        public ImageSource IconSource { get; set; }

        public AppTag _AppTag { get; set; }
        public string StrAppTag { get => AppTagHelper.GetTagDisplayName(this._AppTag); }
        public Brush BrushAppTag { get => AppTagHelper.GetTagColor(this._AppTag); }

        public AppUsageListItem(string processName, string programName, TimeSpan duration, int percentage, AppTag appTag)
        {
            ProcessName = processName;
            ProgramName = programName;
            Duration = duration;
            Percentage = percentage;
            IconSource = IconManager.GetIconSource(processName);
            _AppTag = appTag;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Refresh()
        {
            OnPropertyChanged(nameof(ProcessName));
            OnPropertyChanged(nameof(Percentage));

            OnPropertyChanged(nameof(Duration));
            OnPropertyChanged(nameof(StrDuration));

            OnPropertyChanged(nameof(_AppTag));
            OnPropertyChanged(nameof(StrAppTag));
            OnPropertyChanged(nameof(BrushAppTag));
        }

    }
}
