using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using De.TorstenMandelkow.MetroChart;
using DigitalWellbeingUI.Models;
using DigitalWellbeingUI.Models.UserControls;
using DigitalWellbeingUI.ViewModels;

namespace DigitalWellbeingUI.Views
{
    /// <summary>
    /// Interaction logic for DayAppUsagePage.xaml
    /// </summary>
    public partial class DayAppUsagePage : Page
    {
        AppUsageViewModel vm;

        public DayAppUsagePage()
        {
            InitializeComponent();

            vm = (AppUsageViewModel)DataContext;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            vm.OpenLogsFolder();
        }

        private void BtnPreviousDay_Click(object sender, RoutedEventArgs e)
        {
            vm.LoadPreviousDay();
        }

        private void BtnNextDay_Click(object sender, RoutedEventArgs e)
        {
            vm.LoadNextDay();
        }

        private void AppUsageListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                appUsageChart.SelectedItem = vm.OnAppUsageListView_SelectionChanged((AppUsageListItem)e.AddedItems[0]);
            }
        }

        private void appUsageChart_MouseUp(object sender, MouseButtonEventArgs e)
        {
            PieChart chart = (PieChart)sender;
            ChartDataPoint point = (ChartDataPoint)chart.SelectedItem;
            AppUsageListView.SelectedItem = vm.OnAppUsagePieChart_SelectionChanged(point);
        }
    }
}
