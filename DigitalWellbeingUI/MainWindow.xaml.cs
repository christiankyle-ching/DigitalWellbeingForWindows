using System.Windows;
using System.Linq;
using DigitalWellbeingUI.Views;
using ModernWpf.Controls;
using System.Collections;
using System.Diagnostics;
using System;

namespace DigitalWellbeingUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Pages
        DayAppUsagePage usagePage = new DayAppUsagePage();
        SettingsPage settingsPage = new SettingsPage();

        public MainWindow()
        {
            InitializeComponent();

            NavView.SelectedItem = NavView.MenuItems[0];
        }

        private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            NavigationViewItem selectedNavItem = args.SelectedItem as NavigationViewItem;

            switch (selectedNavItem.Tag)
            {
                case "home":
                    NavView.Header = "App Usage";
                    ContentFrame.Content = usagePage;
                    break;
                case "Settings":
                    NavView.Header = "Settings";
                    ContentFrame.Content = settingsPage;
                    break;
            }

        }
    }
}
