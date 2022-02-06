using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Environment;

namespace DigitalWellbeing.Core
{
    public static class ApplicationPath
    {
        static readonly SpecialFolder applicationPath = SpecialFolder.LocalApplicationData;
        static readonly string applicationFolderName = "digital-wellbeing";
        static readonly string imageCacheFolderName = "processicons";
        static readonly string dailyLogsFolderName = "dailylogs";
        static readonly string internalLogsFolder = "internal-logs";
        static readonly string settingsFolder = "settings";
        static readonly string autorunFileName = ".autorun";

        public static readonly string AUTORUN_REGPATH = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run";
#if DEBUG
        public static readonly string AUTORUN_REGKEY = "DigitalWellbeingWPFDEBUG";
#else
        public static readonly string AUTORUN_REGKEY = "DigitalWellbeingWPF";
#endif



        public static string APP_LOCATION
        {
            get => GetFolderPath(applicationPath) + $@"\{applicationFolderName}";
        }

        public static string autorunFilePath
        {
            get => APP_LOCATION + $@"\{autorunFileName}";
        }

        public static string UsageLogsFolder
        {
            get => APP_LOCATION + $@"\{dailyLogsFolderName}\";
        }

        public static string SettingsFolder
        {
            get => APP_LOCATION + $@"\{settingsFolder}\";
        }

        public static string InternalLogsFolder
        {
            get => APP_LOCATION + $@"\{internalLogsFolder}\";
        }

        public static string GetImageCacheLocation(string appName = "")
        {
            string location = APP_LOCATION + $@"\{imageCacheFolderName}\";
            if (appName != "") { location += $"{appName}.ico"; }
            return location;
        }
    }

}
