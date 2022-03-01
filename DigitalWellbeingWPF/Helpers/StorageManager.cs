using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalWellbeingWPF.Helpers
{
    public static class StorageManager
    {
        public static bool TryDeleteFolder(string folderPath)
        {
            try
            {
                Directory.Delete(folderPath, true);
                return true;
            }
            catch (Exception ex)
            {
                AppLogger.WriteLine(ex.Message);
                return false;
            }
        }


        private static string GetFolderSize(string folderPath)
        {
            try
            {
                Process cmd = new Process();
                cmd.StartInfo.FileName = "cmd.exe";
                cmd.StartInfo.RedirectStandardInput = true;
                cmd.StartInfo.RedirectStandardOutput = true;
                cmd.StartInfo.CreateNoWindow = true;
                cmd.StartInfo.UseShellExecute = false;
                cmd.Start();

                cmd.StandardInput.WriteLine($@"dir /s {folderPath}");
                cmd.StandardInput.Flush();
                cmd.StandardInput.Close();

                string _out = cmd.StandardOutput.ReadToEnd();
                cmd.WaitForExit();

                string[] output = _out.Replace("\r", "").Split('\n');
                string[] lineWithTotal = output[output.Length - 4].Split(' ');
                ulong totalBytes = ulong.Parse(lineWithTotal[lineWithTotal.Length - 2].Replace(",", ""));

                string strSize = StringHelper.ShortenBytes(totalBytes); // Divide by 1MB in bytes

                return strSize;
            }
            catch (Exception ex)
            {
                // Not so important
                AppLogger.WriteLine(ex);

                return "";
            }
        }
    }
}
