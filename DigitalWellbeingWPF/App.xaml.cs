using DigitalWellbeingWPF.Helpers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace DigitalWellbeingWPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Process thisProcess = Process.GetCurrentProcess();

            IEnumerable<Process> similarAppProcesses = Process.GetProcesses().Where(p => p.ProcessName == thisProcess.ProcessName);

            if (similarAppProcesses.Count() > 1)
            {
                try
                {
                    IntPtr existingProcessHWnd = similarAppProcesses.Single(p => thisProcess.Id != p.Id).MainWindowHandle;
                    ShowWindow(existingProcessHWnd, 9);
                    bool isSet = SetForegroundWindow(existingProcessHWnd);
                    if (!isSet) { ShowMessageBox();  }
                    App.Current.Shutdown();
                }
                catch
                {
                    AppLogger.WriteLine("Didn't catch the existing process.");
                    ShowMessageBox();
                }
            }
        }

        private void ShowMessageBox()
        {
            MessageBox.Show("Application is already running...");
        }

        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
    }
}
