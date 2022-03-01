using DigitalWellbeing.Core;
using DigitalWellbeingWPF.Helpers;
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
using System.Windows.Shapes;

namespace DigitalWellbeingWPF.Views
{
    /// <summary>
    /// Interaction logic for ClearData.xaml
    /// </summary>
    public partial class ClearDataWindow : Window
    {
        private static string NL = StringHelper.NEWLINE;

        public ClearDataWindow()
        {
            InitializeComponent();
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            List<string> selectedOpts = new List<string>();

            string message = $"Are you sure you want to delete the following:{NL}{NL}";

            if (chkDailyLogs.IsChecked == true) selectedOpts.Add($"• Daily Logs");
            if (chkInternalLogs.IsChecked == true) selectedOpts.Add($"• Internal Logs");
            if (chkProcessIcons.IsChecked == true) selectedOpts.Add($"• App Icons");
            message += string.Join(NL, selectedOpts);

            message += $"{NL}{NL}THIS ACTION CANNOT BE UNDONE!";

            MessageBoxResult res = MessageBox.Show(message, "Confirm Clear Data",
                MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No);

            if (res == MessageBoxResult.Yes)
            {
                try
                {
                    ClearSelected();
                }
                catch (Exception ex)
                {
                    AppLogger.WriteLine(ex);
                }
                finally
                {
                    if (chkDailyLogs.IsChecked == true)
                    {
                        MainWindow mWindow = Application.Current.MainWindow as MainWindow;
                        mWindow.ReloadUsagePage();
                    }

                    this.Close();
                }
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ClearSelected()
        {
            if (chkDailyLogs.IsChecked == true)
            {
                StorageManager.TryDeleteFolder(ApplicationPath.UsageLogsFolder);
            }

            if (chkInternalLogs.IsChecked == true)
            {
                StorageManager.TryDeleteFolder(ApplicationPath.InternalLogsFolder);
            }

            if (chkProcessIcons.IsChecked == true)
            {
                IconManager.ClearCachedImages();
            }
        }

        private void Options_Changed(object sender, RoutedEventArgs e)
        {
            bool enabled =
                (chkDailyLogs.IsChecked ?? false) ||
                (chkInternalLogs.IsChecked ?? false) ||
                (chkProcessIcons.IsChecked ?? false);

            btnDelete.IsEnabled = enabled;
        }
    }
}
