using DigitalWellbeingWPF.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Shapes;

namespace DigitalWellbeingWPF.Views
{
    /// <summary>
    /// Interaction logic for AboutTheApp.xaml
    /// </summary>
    public partial class AboutTheApp : Window
    {
        private readonly int currentVersion;
        private readonly string githubLink = "https://github.com/christiankyle-ching/DigitalWellbeingForWindows";
        private readonly string updateLink = "https://github.com/christiankyle-ching/DigitalWellbeingForWindows/releases";
        private readonly string websiteLink = "https://christiankyleching.vercel.app/works.html?scrollTo=digital-wellbeing-windows";

        public AboutTheApp()
        {
            InitializeComponent();

            string strVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            TxtVersion.Text = $"Version {strVersion}";

            currentVersion = Updater.ParseVersion(strVersion);
            CheckForUpdates();
        }

        private async void CheckForUpdates()
        {
            string latestVersion = await Updater.GetLatestVersion();

            if (Updater.IsUpdateAvailable(currentVersion, Updater.ParseVersion(latestVersion)))
            {
                TxtVersion.Text += $"\nUpdate Available ({latestVersion})";
            }
        }

        private void BtnGithub_Click(object sender, RoutedEventArgs e)
        {
            _ = Process.Start(githubLink);
        }

        private void BtnWebsite_Click(object sender, RoutedEventArgs e)
        {
            _ = Process.Start(websiteLink);
        }

        private void BtnCheckUpdates_Click(object sender, RoutedEventArgs e)
        {
            _ = Process.Start(updateLink);
        }
    }
}
