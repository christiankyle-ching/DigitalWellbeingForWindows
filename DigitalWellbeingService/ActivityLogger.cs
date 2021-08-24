using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Timers;
using Topshelf;

namespace DigitalWellbeingService
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
            DateTime _dateTime = DateTime.Now;
            uint currProcessId = GetForegroundProcessId();

            if (IsProcessChanged(currProcessId))
            {
                Console.WriteLine("CHANGED");
                lastProcessId = currProcessId;

                string[] newProcessLine = new string[] {
                    $"{_dateTime}\t{GetActiveProcessName(currProcessId)}",
                    $"{_dateTime}\t{IND_LAST}"
                };

                AppendLineToFile(newProcessLine);
            }
            else
            {
                Console.WriteLine("NO CHANGE");

                string[] ind_lastLine = new string[] {
                    $"{_dateTime}\t{IND_LAST}"
                };

                AppendLineToFile(ind_lastLine);
            }
        }

        private void AppendLineToFile(string[] lines)
        {
            string filePath = $"{folderPath}{DateTime.Now:MM-dd-yyyy}.log";
            bool isInsertingProcess = lines.Length >= 2;

            try
            {
                List<string> fileLines = File.ReadAllLines(filePath).ToList();

                // Do not add {IND_LAST} if there are no logged process
                if (!isInsertingProcess && fileLines.Count <= 0) { return; }

                fileLines.RemoveRange(fileLines.Count - 1, 1);
                fileLines.AddRange(lines);

                File.WriteAllLines(filePath, fileLines);
            }
            catch (DirectoryNotFoundException)
            {
                Directory.CreateDirectory(folderPath);

                //First entry, insert 2 lines(total 3 lines incl.last newline)
                if (isInsertingProcess)
                {
                    File.AppendAllLines(filePath, lines);
                }
            }
            catch (FileNotFoundException)
            {
                //First entry, insert 2 lines(total 3 lines incl.last newline)
                if (isInsertingProcess)
                {
                    File.AppendAllLines(filePath, lines);
                }
            }
            catch (IOException ex)
            {
                Console.WriteLine(ex);
                // File might be currently read by UI application for auto-refresh.
                return;
            }
        }

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        static extern IntPtr GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        private uint GetForegroundProcessId()
        {
            IntPtr handle = GetForegroundWindow();
            GetWindowThreadProcessId(handle, out uint processId);

            return processId;
        }

        private string GetActiveProcessName(uint processId)
        {
            Process p = Process.GetProcessById((int)processId);
            return p.ProcessName;
        }

        private bool IsProcessChanged(uint processId)
        {
            return processId != lastProcessId;
        }
    }
}
