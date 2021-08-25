using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DigitalWellbeingService.NET4._6
{
    public class ActivityLogger
    {
        private static readonly string IND_LAST = "*LAST";

        private const string envFolderPath = @"%USERPROFILE%\.digitalwellbeing\dailylogs\";
        private string folderPath;

        private uint lastProcessId = 0;

        public ActivityLogger()
        {
            folderPath = Environment.ExpandEnvironmentVariables(envFolderPath);
        }

        public void OnTimer()
        {
            IntPtr handle = GetForegroundWindow();
            uint currProcessId = GetForegroundProcessId(handle);
            Process proc = Process.GetProcessById((int)currProcessId);

            if (IsProcessChanged(currProcessId))
            {
                string newProcessName = GetActiveProcessName(proc);
                string newProgramName = GetActiveProgramName(proc);

                if (newProgramName == null && newProcessName == null)
                {
                    // Error in getting process or program name.
                    UpdateLastLine();
                }
                else
                {
                    lastProcessId = currProcessId;
                    UpdateLastLine(newProcessName, newProgramName);
                }
            }
            else
            {
                UpdateLastLine();
            }
        }

        private void UpdateLastLine(string processName = "", string programName = "")
        {
            DateTime _dateTime = DateTime.Now;
            string lastLine = $"{_dateTime}\t{IND_LAST}\t{IND_LAST}";

            if (programName == "" && processName == "")
            {
                string[] ind_lastLine = new string[] {
                    lastLine
                };

                AppendLineToFile(ind_lastLine);
            }
            else
            {
                string[] newProcessLine = new string[] {
                    $"{_dateTime}\t{processName}\t{programName}",
                    lastLine
                };

                AppendLineToFile(newProcessLine);
            }

        }

        private void AppendLineToFile(string[] linesToInsert)
        {
            string filePath = $"{folderPath}{DateTime.Now:MM-dd-yyyy}.log";
            bool isInsertingProcess = linesToInsert.Length >= 2;

            try
            {
                List<string> existingLines = File.ReadAllLines(filePath).ToList();

                // Prevent having *LAST on first line
                if (!isInsertingProcess && existingLines.Count <= 0) { return; }

                // If file already exists but no lines detected
                if (isInsertingProcess && existingLines.Count <= 0)
                {
                    File.AppendAllLines(filePath, linesToInsert);
                    return;
                }

                existingLines.RemoveRange(existingLines.Count - 1, 1);
                existingLines.AddRange(linesToInsert);

                File.WriteAllLines(filePath, existingLines);
            }
            catch (DirectoryNotFoundException)
            {
                Directory.CreateDirectory(folderPath);

                //First entry, insert 2 lines(total 3 lines incl.last newline)
                if (isInsertingProcess)
                {
                    File.AppendAllLines(filePath, linesToInsert);
                }
            }
            catch (FileNotFoundException)
            {
                //First entry, insert 2 lines(total 3 lines incl.last newline)
                if (isInsertingProcess)
                {
                    File.AppendAllLines(filePath, linesToInsert);
                }
            }
            catch (IOException ex)
            {
                Console.WriteLine(ex);
                // File might be currently read by UI application for auto-refresh.
                return;
            }
        }

        private uint GetForegroundProcessId(IntPtr handle)
        {
            GetWindowThreadProcessId(handle, out uint processId);

            return processId;
        }

        private string GetActiveProcessName(Process p)
        {


            try
            {
                return p.ProcessName;
            }
            catch
            {
                return null;
            }
        }

        private string GetActiveProgramName(Process p)
        {
            try
            {
                return p.MainModule.FileVersionInfo.ProductName;
            }
            catch
            {
                return null;
            }
        }

        private bool IsProcessChanged(uint processId)
        {
            return processId != lastProcessId;
        }

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        static extern IntPtr GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        [DllImport("user32.dll")]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

    }
}
