using DigitalWellbeingWPF.Helpers;
using DigitalWellbeingWPF.Views;
using ModernWpf.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
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
        DayAppUsagePage usagePage = new DayAppUsagePage();
        SettingsPage settingsPage = new SettingsPage();

        //NotifyIcon trayIcon = new NotifyIcon();

        public MainWindow()
        {
            InitializeComponent();

            // Navigate to Home
            this.NavView.SelectedItem = this.NavView.MenuItems[0];

            // Setup Tray Icon
            //string exePath = System.Reflection.Assembly.GetEntryAssembly().ManifestModule.Name;
            //trayIcon.Icon = System.Drawing.Icon.ExtractAssociatedIcon(exePath);
            //trayIcon.Visible = false;
            //trayIcon.Text = "Digital Wellbeing For Windows is running in the background.";
            //trayIcon.DoubleClick += (sender, args) =>
            //{
            //    this.Show();
            //    trayIcon.Visible = false;
            //    this.WindowState = WindowState.Normal;
            //};
        }

        private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            NavigationViewItem selectedNavItem = args.SelectedItem as NavigationViewItem;

            if (args.IsSettingsSelected)
            {
                NavView.Header = "Settings";
                ContentFrame.Content = settingsPage;
                return;
            }

            switch (selectedNavItem.Tag)
            {
                case "home":
                    NavView.Header = "App Usage (Last 7 Days)";
                    ContentFrame.Content = usagePage;
                    usagePage.RefreshWithSettings();
                    break;
                default:
                    NavView.Header = "App Usage (Last 7 Days)";
                    ContentFrame.Content = usagePage;
                    usagePage.RefreshWithSettings();
                    break;
            }
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            //if (this.WindowState == WindowState.Minimized)
            //{
            //    trayIcon.Visible = true;
            //    this.Hide();
            //}
        }
    }
}
