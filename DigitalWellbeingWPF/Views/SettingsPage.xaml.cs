using DigitalWellbeing.Core;
using DigitalWellbeingWPF.Helpers;
using Microsoft.Win32;
using ModernWpf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
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
        private readonly ApplicationTheme? systemTheme;

        private int UPDATE_CHECK_DELAY = 20;
        const string APP_TIMELIMIT_SEPARATOR = "    /    ";

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

            LoadAboutApp();
        }

        public void OnNavigate()
        {
            LoadExcludedProcessItems();
            LoadAppTimeLimits();
        }

        #region Loader Functions

        private void LoadCurrentSettings()
        {
            TimeSpan minDuration = Properties.Settings.Default.MinumumDuration;
            MinDuration_Hours.Value = minDuration.Hours;
            MinDuration_Minutes.Value = minDuration.Minutes;
            MinDuration_Seconds.Value = minDuration.Seconds;

            EnableRunOnStartup.IsOn = SettingsManager.IsRunningOnStartup();
            ToggleMinimizeOnExit.IsOn = Properties.Settings.Default.MinimizeOnExit;

            EnableAutoRefresh.IsOn = Properties.Settings.Default.EnableAutoRefresh;
            RefreshInterval.Value = Properties.Settings.Default.RefreshIntervalSeconds;

            CBTheme.SelectedItem = CBTheme.FindName($"CBTheme_{Properties.Settings.Default.ThemeMode}");

            LoadExcludedProcessItems();
            LoadAppTimeLimits();
        }

        private void LoadExcludedProcessItems()
        {
            ExcludedAppList.Items.Clear();

            string[] excludedProcesses = Properties.Settings.Default.UserExcludedProcesses.Cast<string>().ToArray();

            foreach (string processName in excludedProcesses)
            {
                ExcludedAppList.Items.Add(processName);
            }
        }

        private void LoadAppTimeLimits()
        {
            AppTimeLimitsList.Items.Clear();

            foreach (KeyValuePair<string, int> timeLimit in SettingsManager.appTimeLimits)
            {
                TimeSpan time = TimeSpan.FromMinutes(timeLimit.Value);
                AppTimeLimitsList.Items.Add($"{timeLimit.Key}{APP_TIMELIMIT_SEPARATOR}{time.Hours}h {time.Minutes}m");
            }
        }
        #endregion

        #region Events
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

        private void MinDuration_LostFocus(object sender, RoutedEventArgs e)
        {
            int hrs = (int)MinDuration_Hours.Value;
            int min = (int)MinDuration_Minutes.Value;
            int sec = (int)MinDuration_Seconds.Value;

            hrs = hrs < 0 ? 0 : hrs;
            min = min < 0 ? 0 : min;
            sec = sec < 0 ? 0 : sec;

            MinDuration_Hours.Value = hrs;
            MinDuration_Minutes.Value = min;
            MinDuration_Seconds.Value = sec;

            Properties.Settings.Default.MinumumDuration = new TimeSpan(hrs, min, sec);
            Properties.Settings.Default.Save();
        }

        private void ExcludedAppList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                ListView list = (ListView)sender;

                string processName = list.SelectedItem.ToString();

                Properties.Settings.Default.UserExcludedProcesses.Remove(processName);
                Properties.Settings.Default.Save();

                list.Items.Remove(list.SelectedItem);
            }
            catch (NullReferenceException ex)
            {
                Console.WriteLine($"No item selected: {ex}");
            }
        }

        private void BtnFocusAssist_Click(object sender, RoutedEventArgs e)
        {
            _ = Process.Start("ms-settings:quiethours");
            _ = Process.Start("ms-settings:quietmomentshome");
        }

        private void AppTimeLimitsList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                ListView list = (ListView)sender;

                string processName = list.SelectedItem.ToString().Split(new[] { APP_TIMELIMIT_SEPARATOR }, StringSplitOptions.None)[0];

                SetTimeLimitWindow window = new SetTimeLimitWindow(processName);
                window.ShowDialog();

                LoadAppTimeLimits();
                Notifier.ResetNotificationForApp(processName);
            }
            catch (NullReferenceException ex)
            {
                Console.WriteLine($"No item selected: {ex}");
            }
        }

        private void EnableRunOnStartup_Toggled(object sender, RoutedEventArgs e)
        {
            SettingsManager.SetRunOnStartup(EnableRunOnStartup.IsOn);
        }

        private void ToggleMinimizeOnExit_Toggled(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.MinimizeOnExit = ToggleMinimizeOnExit.IsOn;
            Properties.Settings.Default.Save();
        }

        #endregion

        #region About App

        private void LoadAboutApp()
        {
            LoadLinks();

            Assembly app = Assembly.GetExecutingAssembly();

            // Get Copyright
            object[] attribs = app.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), true);
            TxtCopyright.Text = (attribs.Length > 0) ? ((AssemblyCopyrightAttribute)attribs[0]).Copyright : "";

            // Get Version
            string strVersion = app.GetName().Version.ToString();
            TxtCurrentVersion.Text = $"App version {strVersion}";

            DelayCheckForUpdates();
        }

        private async void DelayCheckForUpdates()
        {
            await Task.Delay(UPDATE_CHECK_DELAY * 1000);

            CheckForUpdates();
        }

        private async void CheckForUpdates(bool manualRefresh = false)
        {
            string latestVersion = await Updater.CheckForUpdates();

            if (latestVersion != "")
            {
                TxtLatestVersion.Text = $" ({latestVersion})";

                Notifier.ShowNotification(
                App.APPNAME,
                    $"Update Available: {latestVersion}",
                    (s, e) => { _ = Process.Start(Updater.appReleasesLink); }
                    );
            }
            else
            {
                if (manualRefresh)
                {
                    Notifier.ShowNotification(App.APPNAME, "No updates available.");
                }
            }
        }

        private void BtnCheckUpdate_Click(object sender, RoutedEventArgs e)
        {
            CheckForUpdates(true);
        }

        private void LoadLinks()
        {
            LinkSource.NavigateUri = new Uri(Updater.appGithubLink);
            LinkUpdate.NavigateUri = new Uri(Updater.appReleasesLink);
            LinkDeveloper.NavigateUri = new Uri(Updater.appWebsiteLink);
        }
        #endregion

        private void BtnOpenAppFolder_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(ApplicationPath.APP_LOCATION);
        }

        private void BtnClearData_Click(object sender, RoutedEventArgs e)
        {
            ClearDataWindow wnd = new ClearDataWindow();
            wnd.ShowDialog();
        }
    }
}
