using DigitalWellbeingWPF.Models.UserControls;
using DigitalWellbeingWPF.ViewModels;
using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace DigitalWellbeingWPF.Views
{
    /// <summary>
    /// Interaction logic for DayAppUsagePage.xaml
    /// </summary>
    public partial class DayAppUsagePage : Page
    {
        private readonly AppUsageViewModel vm;

        public DayAppUsagePage()
        {
            InitializeComponent();

            vm = (AppUsageViewModel)DataContext;
        }

        private void BtnPreviousDay_Click(object sender, RoutedEventArgs e)
        {
            vm.LoadPreviousDay();
        }

        private void BtnNextDay_Click(object sender, RoutedEventArgs e)
        {
            vm.LoadNextDay();
        }

        private void AppUsageChart_DataClick(object sender, LiveCharts.ChartPoint chartPoint)
        {
            AppUsageListItem existingListItem = vm.OnAppUsageChart_SelectionChanged(chartPoint);

            AppUsageListItem listItem;
            ModernWpf.Controls.ListViewItem listViewItemElement;

            Debug.WriteLine(AppUsageViewModel.MaximumChartSeries);
            Debug.WriteLine(AppUsageListView.Items.Count);

            if (existingListItem == null && chartPoint.SeriesView.Title == "Other Apps" && AppUsageListView.Items.Count > AppUsageViewModel.MaximumChartSeries)
            {
                listItem = (AppUsageListItem)AppUsageListView.Items[AppUsageViewModel.MaximumChartSeries];
                listViewItemElement = (ModernWpf.Controls.ListViewItem)AppUsageListView.ItemContainerGenerator.ContainerFromItem(listItem);
            }
            else
            {
                listItem = existingListItem;
                listViewItemElement = (ModernWpf.Controls.ListViewItem)AppUsageListView.ItemContainerGenerator.ContainerFromItem(existingListItem);
            }

            AppUsageListView.SelectedItem = listItem;
            listViewItemElement.Focus();
        }

        private void WeeklyChart_DataClick(object sender, LiveCharts.ChartPoint chartPoint)
        {
            vm.WeeklyChart_SelectionChanged((int)chartPoint.X);
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            vm.OnPageResize(appUsageChart.ActualWidth, appUsageChart.ActualHeight);
        }

        private void AppUsageListMenuItem_Click(object sender, RoutedEventArgs e)
        {
            string processName = ((MenuItem)sender).Tag.ToString();
            Properties.Settings.Default.UserExcludedProcesses.Add(processName);
            Properties.Settings.Default.Save();

            vm.OnExcludeApp(processName);
        }

        public void OnNavigate()
        {
            vm.OnNavigate();
        }
    }
}
