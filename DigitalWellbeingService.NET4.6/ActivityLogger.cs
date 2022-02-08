using DigitalWellbeing.Core;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static System.Environment;

namespace DigitalWellbeingService.NET4._6
{
    public class ActivityLogger
    {
        public static readonly int TIMER_INTERVAL_SEC = 3;

        private string folderPath;
        private string autoRunFilePath;

        public ActivityLogger()
        {
            folderPath = ApplicationPath.UsageLogsFolder;
            autoRunFilePath = ApplicationPath.autorunFilePath;

            Debug.WriteLine(folderPath);
            Debug.WriteLine(autoRunFilePath);

            TryCreateAutoRunFile();
        }

        // AutoRun only
        private void TryCreateAutoRunFile()
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey(ApplicationPath.AUTORUN_REGPATH);

            bool isAutoRun = key.GetValue(ApplicationPath.AUTORUN_REGKEY) != null ? true : false;

            // Create an empty file that UI will check, do startup things (like hiding window) and delete.
            if (isAutoRun) File.Create(autoRunFilePath).Dispose();
        }

        // Main Timer Logic
        public void OnTimer()
        {
            IntPtr handle = ForegroundWindowManager.GetForegroundWindow();
            uint currProcessId = ForegroundWindowManager.GetForegroundProcessId(handle);
            Process proc = Process.GetProcessById((int)currProcessId);

            UpdateTimeEntry(proc);
        }

        private void UpdateTimeEntry(Process proc)
        {
            string filePath = $"{folderPath}{DateTime.Now:MM-dd-yyyy}.log";

            try
            {
                List<string> lines = File.ReadAllLines(filePath).ToList();

                bool found = false;

                // Update Time Entry
                for (int i = 0; i < lines.Count; i++)
                {
                    if (lines[i].Trim() == string.Empty) continue;

                    string[] cells = lines[i].Split('\t');

                    string processName = cells[0];

                    // If already found, update and break
                    if (proc.ProcessName == processName)
                    {
                        int seconds = 0;
                        string programName = cells.Length > 2 ? cells[2] : "";

                        // Try get seconds
                        int.TryParse(cells[1], out seconds);

                        // Just update the array inline then break
                        seconds += TIMER_INTERVAL_SEC;
                        lines[i] = GetEntryRow(processName, seconds, programName);

                        found = true;
                        break;
                    }
                }

                // If not found, then add at end with starting seconds as interval
                if (!found)
                {
                    string newProcessName = ForegroundWindowManager.GetActiveProcessName(proc);
                    string newProgramName = ForegroundWindowManager.GetActiveProgramName(proc);

                    lines.Add(GetEntryRow(newProcessName, TIMER_INTERVAL_SEC, newProgramName));
                }

                // Update the file again
                File.WriteAllLines(filePath, lines);
            }
            catch (DirectoryNotFoundException)
            {
                Directory.CreateDirectory(folderPath);
            }
            catch (FileNotFoundException)
            {
                // Create empty file
                File.AppendAllLines(filePath, new List<string>());
            }
            catch (IOException ex)
            {
                Console.WriteLine(ex);
                // File might be currently read by UI application for auto-refresh.
                return;
            }
        }

        private string GetEntryRow(string processName, int seconds, string programName)
        {
            return $"{processName}\t{seconds}\t{programName}";
        }
    }
}
