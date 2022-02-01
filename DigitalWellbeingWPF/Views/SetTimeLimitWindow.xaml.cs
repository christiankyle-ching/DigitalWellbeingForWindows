using DigitalWellbeingWPF.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static DigitalWellbeingWPF.Helpers.NumberFormatter;

namespace DigitalWellbeingWPF.Views
{
    /// <summary>
    /// Interaction logic for AddTimeLimit.xaml
    /// </summary>
    public partial class SetTimeLimitWindow : Window
    {
        private string pName = "";

        public SetTimeLimitWindow(string processName)
        {
            InitializeComponent();

            pName = processName;
            this.Title = $"Set Time Limit ({processName})";

            InitFormatter();
            InitValues();
        }

        private void InitFormatter()
        {
            ModernWpf.Controls.INumberBoxNumberFormatter formatter = new WholeNumberFormatter();
            DurationHours.NumberFormatter = formatter;
            DurationMinutes.NumberFormatter = formatter;
        }

        private void InitValues()
        {
            if (SettingsManager.appTimeLimits.ContainsKey(pName))
            {
                TimeSpan timeLimit = TimeSpan.FromMinutes(SettingsManager.appTimeLimits[pName]);
                DurationHours.Value = timeLimit.Hours;
                DurationMinutes.Value = timeLimit.Minutes;
            }
            else
            {
                DurationHours.Value = 0;
                DurationMinutes.Value = 0;
            }
        }

        private void Duration_LostFocus(object sender, RoutedEventArgs e)
        {
            int hrs = (int)DurationHours.Value;
            int min = (int)DurationMinutes.Value;

            hrs = hrs < 0 ? 0 : hrs;
            min = min < 0 ? 0 : min;

            DurationHours.Value = hrs;
            DurationMinutes.Value = min;
        }

        private void BtnApply_Click(object sender, RoutedEventArgs e)
        {
            int hrs = (int)DurationHours.Value;
            int min = (int)DurationMinutes.Value;

            SettingsManager.UpdateAppTimeLimit(pName, new TimeSpan(hrs, min, 0));

            this.Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
