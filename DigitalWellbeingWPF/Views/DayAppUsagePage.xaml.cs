using DigitalWellbeingWPF.Helpers;
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

            AppUsageListItem listItem = null;
            ModernWpf.Controls.ListViewItem listViewItemElement = null;

            try
            {
                if (existingListItem == null && chartPoint.SeriesView.Title == "Other Apps")
                {
                    if (chartPoint.SeriesView.Title == "Other Apps")
                    {
                        listItem = AppUsageListView.Items.Cast<AppUsageListItem>().ToArray().First(item => item.Percentage <= AppUsageViewModel.MinimumPieChartPercentage);
                        listViewItemElement = (ModernWpf.Controls.ListViewItem)AppUsageListView.ItemContainerGenerator.ContainerFromItem(listItem);
                    }
                    else if (chartPoint.SeriesView.Title == "No Data" && chartPoint.Y == 1.0)
                    {
                        return; // No Data
                    }
                }
                else
                {
                    listItem = existingListItem;
                    listViewItemElement = (ModernWpf.Controls.ListViewItem)AppUsageListView.ItemContainerGenerator.ContainerFromItem(existingListItem);
                }

                AppUsageListView.SelectedItem = listItem;
                listViewItemElement.Focus();
            }
            catch (NullReferenceException)
            {
                // Cannot focus on any list item.
            }
            catch (Exception ex)
            {
                AppLogger.WriteLine(ex);
            }
        }

        private void WeeklyChart_DataClick(object sender, LiveCharts.ChartPoint chartPoint)
        {
            vm.WeeklyChart_SelectionChanged((int)chartPoint.X);
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            vm.OnPageResize(appUsageChart.ActualWidth, appUsageChart.ActualHeight);
        }

        private void AppUsageListMenuItem_ExcludeItem(object sender, RoutedEventArgs e)
        {
            string processName = ((MenuItem)sender).Tag.ToString();
            Properties.Settings.Default.UserExcludedProcesses.Add(processName);
            Properties.Settings.Default.Save();

            vm.OnExcludeApp(processName);
        }

        private void AppUsageListMenuItem_SetTimeLimit(object sender, RoutedEventArgs e)
        {
            string processName = ((MenuItem)sender).Tag.ToString();
            vm.OnSetTimeLimit(processName);
        }

        private void AppUsageListMenuItem_SetAppTag(object sender, RoutedEventArgs e)
        {
            string processName = ((MenuItem)sender).Tag.ToString();
            vm.OnSetAppTag(processName);
        }

        public void OnNavigate()
        {
            vm.OnNavigate();
        }

    }
}
