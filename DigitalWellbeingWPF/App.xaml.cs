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
using System.Web;
using System.Windows;
using System.Windows.Threading;

namespace DigitalWellbeingWPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static string APPNAME = "Digital Wellbeing for Windows";
        static string APP_GITHUBISSUE_URL = "https://github.com/christiankyle-ching/DigitalWellbeingForWindows/issues/new?";

        public App()
        {
            // Global Exception Handling
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += new UnhandledExceptionEventHandler(GlobalExceptionHandler);

            // Init Notifier
            Notifier.InitNotifierTimer();
        }

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
                    if (!isSet) { ShowMessage_AlreadyRunning(); }
                    App.Current.Shutdown();
                }
                catch
                {
                    AppLogger.WriteLine("Didn't catch the existing process.");
                    ShowMessage_AlreadyRunning();
                }
            }
        }

        static void GlobalExceptionHandler(object sender, UnhandledExceptionEventArgs args)
        {
            Exception e = (Exception)args.ExceptionObject;
            Console.WriteLine("MyHandler caught : " + e.Message);
            Console.WriteLine("Runtime terminating: {0}", args.IsTerminating);

            ShowMessage_ReportBug(e);
        }

        static void ShowMessage_ReportBug(Exception e)
        {
            string body = HttpUtility.UrlEncode($@"
### Describe how to reproduce the problem:
1. ...
2. ...
3. ...
                
<details>
<summary>Exception Message:</summary>

```
{e.Message}
{e.StackTrace}
```
</details>
");

            string newIssueURL =
                APP_GITHUBISSUE_URL +
                $"body={body}" +
                "&labels=bug";

            Console.WriteLine(newIssueURL);
            AppLogger.WriteLine(e.Message);

            // Process unhandled exception
            MessageBoxResult res = MessageBox.Show(
                "Would you like to report this bug?\n\n" +
                e.Message,
                $"{APPNAME}: Application Error",
                MessageBoxButton.YesNo,
                MessageBoxImage.Error);

            if (res == MessageBoxResult.Yes)
            {
                Process.Start(newIssueURL);
            }
        }

        private void ShowMessage_AlreadyRunning()
        {
            MessageBox.Show(
                "Application is already running...",
                APPNAME,
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
    }
}
