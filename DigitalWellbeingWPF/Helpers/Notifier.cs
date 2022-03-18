using DigitalWellbeingWPF.Models;
using DigitalWellbeingWPF.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using DigitalWellbeing.Core;
using DigitalWellbeingWPF.Views;

namespace DigitalWellbeingWPF.Helpers
{
    public static class Notifier
    {
        public static System.Windows.Forms.NotifyIcon trayIcon;
        private static System.Windows.Forms.ContextMenuStrip ctx;
        private static int NOTIFICATION_TIMOUT_SECONDS = 10;

        private static TimeSpan warningLimit = TimeSpan.FromMinutes(15);

#if DEBUG
        private static int CHECK_INTERVAL = 10;
#else
        private static int CHECK_INTERVAL = 60;
#endif

        private static EventHandler defaultNotificationHandler;

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
                mWindow.ForceClose();
            });

            // Always visible for notifications to work
            trayIcon.Visible = true;
        }

        public static void ShowNotification(string title, string message, EventHandler clickHandler = null, System.Windows.Forms.ToolTipIcon icon = System.Windows.Forms.ToolTipIcon.None)
        {
            trayIcon.BalloonTipTitle = title;
            trayIcon.BalloonTipText = message;
            trayIcon.BalloonTipIcon = icon;

            trayIcon.BalloonTipClicked += clickHandler ?? defaultNotificationHandler;

            trayIcon.ShowBalloonTip(NOTIFICATION_TIMOUT_SECONDS * 1000);
        }

        public static void SetDoubleClickHandler(EventHandler doubleClickHandler)
        {
            trayIcon.DoubleClick += doubleClickHandler;
        }

        public static void SetDefaultNotificationHandler(EventHandler baloonTipHandlerClick)
        {
            defaultNotificationHandler = baloonTipHandlerClick;
            trayIcon.BalloonTipClicked += baloonTipHandlerClick;
        }

        #region App Time Limit Checker

        private static DispatcherTimer notifierTimer;
        private static List<string> notifiedApps = new List<string>();
        private static List<string> warnNotifiedApps = new List<string>();

        public static void InitNotifierTimer()
        {
            TimeSpan intervalDuration = TimeSpan.FromSeconds(CHECK_INTERVAL);

            notifierTimer = new DispatcherTimer() { Interval = intervalDuration };
            notifierTimer.Tick += (s, e) => CheckForExceedingAppTimeLimits();

            notifierTimer.Start();
        }

        private static async void CheckForExceedingAppTimeLimits()
        {
            // Get Source Data
            List<AppUsage> todayUsage = await AppUsageViewModel.GetData(DateTime.Now);
            var _limits = SettingsManager.appTimeLimits;

            // Get Active Process / Program
            IntPtr _hnd = ForegroundWindowManager.GetForegroundWindow();
            uint _procId = ForegroundWindowManager.GetForegroundProcessId(_hnd);
            Process _proc = Process.GetProcessById((int)_procId);
            string activeProcessName = ForegroundWindowManager.GetActiveProcessName(_proc);

            try
            {
                AppUsage currApp = todayUsage.Single(app => app.ProcessName == activeProcessName);

                // Skip if already notified
                if (notifiedApps.Contains(currApp.ProcessName)) return;

                // If app has time limit
                if (_limits.ContainsKey(currApp.ProcessName))
                {
                    TimeSpan timeLimit = TimeSpan.FromMinutes(_limits[currApp.ProcessName]);

                    bool reachedWarnLimit = currApp.Duration > (timeLimit - warningLimit);
                    bool reachedTimeLimit = currApp.Duration > timeLimit;

                    if (reachedTimeLimit && !notifiedApps.Contains(currApp.ProcessName))
                    {
                        warnNotifiedApps.Add(currApp.ProcessName);
                        notifiedApps.Add(currApp.ProcessName);

                        (Application.Current.MainWindow as MainWindow).ShowAlertUsage(currApp, timeLimit);
                    }
                    else if (reachedWarnLimit && !warnNotifiedApps.Contains(currApp.ProcessName))
                    {
                        warnNotifiedApps.Add(currApp.ProcessName);

                        (Application.Current.MainWindow as MainWindow).ShowAlertUsage(currApp, timeLimit, true);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public static void ResetNotificationForApp(string processName)
        {
            notifiedApps.RemoveAll(p => p == processName);
            warnNotifiedApps.RemoveAll(p => p == processName);
        }
        #endregion
    }
}
