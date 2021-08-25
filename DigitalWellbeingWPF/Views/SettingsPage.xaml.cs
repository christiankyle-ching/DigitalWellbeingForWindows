using ModernWpf;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using static DigitalWellbeingWPF.Helpers.NumberFormatter;

namespace DigitalWellbeingWPF.Views
{
    /// <summary>
    /// Interaction logic for SettingsPage.xaml
    /// </summary>
    public partial class SettingsPage : Page
    {
        private ApplicationTheme? systemTheme;

        public SettingsPage()
        {
            InitializeComponent();

            systemTheme = ThemeManager.Current.ApplicationTheme;

            LoadCurrentSettings();

            ModernWpf.Controls.INumberBoxNumberFormatter formatter = new WholeNumberFormatter();
            MinDuration_Hours.NumberFormatter = formatter;
            MinDuration_Minutes.NumberFormatter = formatter;
            MinDuration_Seconds.NumberFormatter = formatter;

            RefreshInterval.NumberFormatter = formatter;
        }

        private void LoadCurrentSettings()
        {
            TimeSpan minDuration = Properties.Settings.Default.MinumumDuration;
            MinDuration_Hours.Value = minDuration.Hours;
            MinDuration_Minutes.Value = minDuration.Minutes;
            MinDuration_Seconds.Value = minDuration.Seconds;

            EnableAutoRefresh.IsOn = Properties.Settings.Default.EnableAutoRefresh;
            RefreshInterval.Value = Properties.Settings.Default.RefreshIntervalSeconds;

            CBTheme.SelectedItem = CBTheme.FindName($"CBTheme_{Properties.Settings.Default.ThemeMode}");
        }

        private void CBTheme_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                string selectedTheme = ((ComboBoxItem)e.AddedItems[0]).Content.ToString();

                switch (selectedTheme)
                {
                    case "Light":
                        ThemeManager.Current.ApplicationTheme = ApplicationTheme.Light;
                        Properties.Settings.Default.ThemeMode = "Light";
                        break;
                    case "Dark":
                        ThemeManager.Current.ApplicationTheme = ApplicationTheme.Dark;
                        Properties.Settings.Default.ThemeMode = "Dark";
                        break;
                    case "System":
                        ThemeManager.Current.ApplicationTheme = systemTheme;
                        Properties.Settings.Default.ThemeMode = "System";
                        break;
                }

                Properties.Settings.Default.Save();
            }
        }

        private void MinDuration_ValueChanged(ModernWpf.Controls.NumberBox sender, ModernWpf.Controls.NumberBoxValueChangedEventArgs args)
        {
            int hrs = (int)MinDuration_Hours.Value;
            int min = (int)MinDuration_Minutes.Value;
            int sec = (int)MinDuration_Seconds.Value;

            Properties.Settings.Default.MinumumDuration = new TimeSpan(hrs, min, sec);
            Properties.Settings.Default.Save();
        }

        private void RefreshInterval_ValueChanged(ModernWpf.Controls.NumberBox sender, ModernWpf.Controls.NumberBoxValueChangedEventArgs args)
        {
            int refreshInterval = (int)sender.Value;

            Properties.Settings.Default.RefreshIntervalSeconds = refreshInterval;
            Properties.Settings.Default.Save();
        }

        private void EnableAutoRefresh_Toggled(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.EnableAutoRefresh = EnableAutoRefresh.IsOn;
            Properties.Settings.Default.Save();
        }

        private void BtnAboutDev_Click(object sender, RoutedEventArgs e)
        {
            new AboutTheDeveloper().ShowDialog();
        }
    }
}
