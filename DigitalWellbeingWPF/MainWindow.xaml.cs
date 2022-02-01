using DigitalWellbeingWPF.Helpers;
using DigitalWellbeingWPF.ViewModels;
using DigitalWellbeingWPF.Views;
using ModernWpf.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
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

namespace DigitalWellbeingWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Pages
        private readonly DayAppUsagePage usagePage = new DayAppUsagePage();
        private readonly SettingsPage settingsPage = new SettingsPage();

        public MainWindow()
        {
            InitializeComponent();

            // Navigate to Home
            this.NavView.SelectedItem = this.NavView.MenuItems[0];
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

        private void MinimizeToTray()
        {
            this.Hide();
            Notifier.ShowTrayIcon(RestoreWindow);
        }

        private void RestoreWindow(object sender, EventArgs args)
        {
            this.Show();
            this.WindowState = WindowState.Normal;
            Notifier.HideTrayIcon();

            // Trigger refresh
            usagePage.OnNavigate();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MessageBoxResult res = MessageBox.Show(
                "Are you sure you want to exit the app? Notifications won't work.",
                App.APPNAME,
                MessageBoxButton.OKCancel,
                MessageBoxImage.Warning,
                MessageBoxResult.Cancel);

            if (res == MessageBoxResult.OK)
            {
                Application.Current.Shutdown();
            }
            else
            {
                e.Cancel = true;
            }
        }

        #endregion
    }
}
