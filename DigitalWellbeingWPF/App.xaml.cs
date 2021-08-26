using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
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
            Process process = Process.GetCurrentProcess();

            int processCount = Process.GetProcesses().Where(p => p.ProcessName == process.ProcessName).Count();

            if (processCount > 1)
            {
                MessageBox.Show("Application is already running...");
                App.Current.Shutdown();
            }
        }
    }
}
