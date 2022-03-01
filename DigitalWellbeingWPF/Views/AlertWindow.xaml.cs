using DigitalWellbeingWPF.Helpers;
using DigitalWellbeingWPF.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Media;
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
    /// Interaction logic for AlertWindow.xaml
    /// </summary>
    public partial class AlertWindow : Window
    {
        private string _processName = "";

        public AlertWindow(AppUsage appUsage, TimeSpan limit)
        {
            InitializeComponent();

            _processName = appUsage.ProcessName;

            ProgramName.Text = appUsage.ProgramName;
            ProcessName.Text = appUsage.ProcessName;
            UsageTime.Text = StringHelper.TimeSpanToShortString(appUsage.Duration);
            TimeLimit.Text = StringHelper.TimeSpanToShortString(limit);

            BtnCloseApp.Content = $"Exit ({appUsage.ProcessName})";

            SystemSounds.Beep.Play();
        }

        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            DialogResult = true;
        }

        private void BtnCloseApp_Click(object sender, RoutedEventArgs e)
        {
            Process[] ps = Process.GetProcessesByName(_processName);

            try
            {
                foreach (Process p in ps)
                {
                    p.CloseMainWindow();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Cannot close app: {ex.Message}");
            }
            finally
            {
                Close();
            }
        }
    }
}
