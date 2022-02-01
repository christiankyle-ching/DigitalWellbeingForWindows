using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DigitalWellbeingWPF.Helpers
{
    public static class Notifier
    {
        public static System.Windows.Forms.NotifyIcon trayIcon;
        private static int NOTIFICATION_TIMOUT_SECONDS = 30;

        static Notifier()
        {
            trayIcon = new System.Windows.Forms.NotifyIcon();
            trayIcon.Icon = System.Drawing.Icon.ExtractAssociatedIcon(
                Assembly.GetEntryAssembly().ManifestModule.Name);

            // Always visible for notifications
            trayIcon.Visible = true;
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
            trayIcon.Visible = true;
        }

        public static void HideTrayIcon()
        {
            trayIcon.Visible = false;
        }
    }
}
