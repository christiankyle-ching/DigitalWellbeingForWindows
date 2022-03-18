using DigitalWellbeing.Core;
using DigitalWellbeingWPF.Helpers;
using DigitalWellbeingWPF.Models;
using DigitalWellbeingWPF.ViewModels;
using DigitalWellbeingWPF.Views;
using ModernWpf.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace DigitalWellbeingWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Pages
        private DayAppUsagePage usagePage = new DayAppUsagePage();
        private SettingsPage settingsPage = new SettingsPage();

        public MainWindow()
        {
            InitializeComponent();

            // Navigate to Home
            this.NavView.SelectedItem = this.NavView.MenuItems[0];

            // Init Notifier
            Notifier.InitNotifierTimer();
            // Set Default Click Handler for any Notification
            Notifier.SetDefaultNotificationHandler((s, e) => RestoreWindow());

            // Check Autorun File
            InitAutoRun();
        }

        private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            NavigationViewItem selectedNavItem = args.SelectedItem as NavigationViewItem;

            if (args.IsSettingsSelected)
            {
                NavView.Header = "Settings";
                settingsPage.OnNavigate();
                ContentFrame.Content = settingsPage;
                return;
            }

            switch (selectedNavItem.Tag)
            {
                case "home":
                    NavView.Header = $"App Usage (Last {AppUsageViewModel.PrevDaysToLoad} Days)";
                    ContentFrame.Content = usagePage;
                    usagePage.OnNavigate();
                    break;
                default:
                    NavView.Header = $"App Usage (Last {AppUsageViewModel.PrevDaysToLoad} Days)";
                    ContentFrame.Content = usagePage;
                    usagePage.OnNavigate();
                    break;
            }
        }

        public void GoToSettings()
        {
            RestoreWindow();
            NavView.SelectedItem = NavView.SettingsItem;
        }

        #region Notifications

        private void Window_StateChanged(object sender, EventArgs e)
        {
            switch (this.WindowState)
            {
                case WindowState.Normal:
                    break;
                case WindowState.Minimized:
                    MinimizeToTray();
                    break;
                case WindowState.Maximized:
                    break;
                default:
                    break;
            }
        }

        public void ShowAlertUsage(AppUsage app, TimeSpan timeLimit, bool warnOnly = false)
        {
            if (warnOnly)
            {
                Notifier.ShowNotification(
                    $"Warning for {app.ProgramName}",
                    $"You have less than 15m using this app. " +
                    $"You've been using this app for {StringHelper.TimeSpanToShortString(app.Duration)}."
                    );
            }
            else
            {
                RestoreWindow();

                AlertWindow alertWnd = new AlertWindow(app, timeLimit);
                alertWnd.WindowState = WindowState.Normal;
                bool? closed = alertWnd.ShowDialog();

                if (closed ?? true) MinimizeToTray();
            }
        }

        public void MinimizeToTray()
        {
            this.Hide();
            Notifier.SetDoubleClickHandler((s, e) => RestoreWindow());
        }

        public void RestoreWindow()
        {
            this.Show();
            this.WindowState = WindowState.Normal;

            // Try Set Foreground, if other apps are on top
            IntPtr currAppHnd = Process.GetCurrentProcess().MainWindowHandle;
            ForegroundWindowManager.SetForegroundWindow(currAppHnd);

            // Trigger refresh
            usagePage.OnNavigate();
        }

        public void ForceClose()
        {
            Application.Current.Shutdown();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (Properties.Settings.Default.MinimizeOnExit)
            {
                MainWindow mWindow = Application.Current.MainWindow as MainWindow;
                mWindow.MinimizeToTray();

                e.Cancel = true;
            }
        }

        #endregion

        private DispatcherTimer autorunTimer;
        private readonly int AUTORUN_CHECK_INTERVAL = 5;
        private readonly int AUTORUN_CHECK_MAX_RETRY = 3;
        private int autorunCheckCount = 0;

        private string autorunFilePath = ApplicationPath.autorunFilePath;

        private void InitAutoRun()
        {
            autorunTimer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(AUTORUN_CHECK_INTERVAL) };
            autorunTimer.Tick += (s, e) => CheckAutoRun();

            autorunTimer.Start();
        }

        private void CheckAutoRun()
        {
            if (autorunCheckCount >= AUTORUN_CHECK_MAX_RETRY)
            {
                autorunTimer.Stop();
                return;
            }

            if (File.Exists(autorunFilePath))
            {
                // TODO
                // Do things here that would only run on login / startup,
                // and NOT on consecutive opens of the app

                MainWindow mWindow = Application.Current.MainWindow as MainWindow;
                mWindow.MinimizeToTray();

                File.Delete(autorunFilePath);
            }

            // Increase Check Count
            autorunCheckCount++;
            Console.WriteLine($"Checked Autorun: {autorunCheckCount}");
        }

        public void ReloadUsagePage()
        {
            // New instance to reload all completely
            usagePage = new DayAppUsagePage();
        }
    }
}
