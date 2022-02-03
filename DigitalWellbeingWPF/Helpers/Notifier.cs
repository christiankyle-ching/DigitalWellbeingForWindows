using DigitalWellbeingWPF.Models;
using DigitalWellbeingWPF.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace DigitalWellbeingWPF.Helpers
{
    public static class Notifier
    {
        public static System.Windows.Forms.NotifyIcon trayIcon;
        private static System.Windows.Forms.ContextMenuStrip ctx;
        private static int NOTIFICATION_TIMOUT_SECONDS = 10;
        private static int CHECK_INTERVAL = 130;

        static Notifier()
        {
            trayIcon = new System.Windows.Forms.NotifyIcon();
            trayIcon.Icon = Properties.Resources.app_logo;

            ctx = new System.Windows.Forms.ContextMenuStrip();
            trayIcon.ContextMenuStrip = ctx;

            MainWindow mWindow = Application.Current.MainWindow as MainWindow;

            // Context Menu : Open App
            ctx.Items.Add("Open", null, (s, e) =>
            {
                mWindow.RestoreWindow();
            });

            // Context Menu : Settings
            ctx.Items.Add("Settings", null, (s, e) =>
            {
                mWindow.GoToSettings();
            });

            // Context Menu : Exit App
            ctx.Items.Add("Exit", null, (s, e) =>
            {
                mWindow.Close();
            });

            // Always visible for notifications
            //trayIcon.Visible = true;
        }

        public static void ShowNotification(string title, string message, System.Windows.Forms.ToolTipIcon icon = System.Windows.Forms.ToolTipIcon.None)
        {
            trayIcon.BalloonTipTitle = title;
            trayIcon.BalloonTipText = message;
            trayIcon.BalloonTipIcon = icon;
            trayIcon.ShowBalloonTip(NOTIFICATION_TIMOUT_SECONDS * 1000);
        }

        public static void ShowTrayIcon(EventHandler handler)
        {
            trayIcon.DoubleClick += handler;
            trayIcon.BalloonTipClicked += handler;
            trayIcon.Visible = true;
        }

        public static void HideTrayIcon()
        {
            trayIcon.Visible = false;
        }

        #region App Time Limit Checker

        private static DispatcherTimer notifierTimer;
        private static List<string> notifiedApps = new List<string>();

        public static void InitNotifierTimer()
        {
            TimeSpan intervalDuration = TimeSpan.FromSeconds(CHECK_INTERVAL);

            notifierTimer = new DispatcherTimer() { Interval = intervalDuration };
            notifierTimer.Tick += (s, e) => CheckForExceedingAppTimeLimits();

            notifierTimer.Start();
        }

        private static async void CheckForExceedingAppTimeLimits()
        {
            try
            {
                List<AppUsage> todayUsage = await AppUsageViewModel.GetData(DateTime.Now);
                var _limits = SettingsManager.appTimeLimits;

                foreach (AppUsage app in todayUsage)
                {
                    // If excluded, don't bother
                    if (AppUsageViewModel.IsProcessExcluded(app.ProcessName)) continue;

                    if (_limits.ContainsKey(app.ProcessName))
                    {
                        if (app.Duration.TotalMinutes > _limits[app.ProcessName])
                        {
                            if (notifiedApps.Contains(app.ProcessName))
                            {
                                // Skip notifying for apps already notified
                                continue;
                            }
                            else
                            {
                                TimeSpan timeLimit = TimeSpan.FromMinutes(_limits[app.ProcessName]);
                                Notifier.ShowNotification(
                                    $"App Usage Warning for {app.ProgramName}",
                                    $"Exceeded the time limit ({timeLimit.Hours}h {timeLimit.Minutes}m). Current usage: {app.Duration.Hours}h {app.Duration.Minutes}m.");

                                notifiedApps.Add(app.ProcessName);

                                // Only one notification per check based on:
                                // https://docs.microsoft.com/en-us/dotnet/api/system.windows.forms.notifyicon.showballoontip?view=netframework-4.6
                                // See Remarks section
                                return;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                AppLogger.WriteLine(
                    $"{e.Message}\n{e.StackTrace}\n" +
                    $"{e.InnerException?.Message}\n{e.InnerException?.StackTrace}");
            }
        }

        public static void ResetNotificationForApp(string processName)
        {
            notifiedApps.Remove(processName);
        }
        #endregion
    }
}
