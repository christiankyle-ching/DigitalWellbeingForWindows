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



        private static string APPLOCATION
        {
            get => GetFolderPath(applicationPath) + $@"\{applicationFolderName}";
        }

        public static string autorunFilePath
        {
            get => APPLOCATION + $@"\{autorunFileName}";
        }

        public static string UsageLogsFolder
        {
            get => APPLOCATION + $@"\{dailyLogsFolderName}\";
        }

        public static string SettingsFolder
        {
            get => APPLOCATION + $@"\{settingsFolder}\";
        }

        public static string InternalLogsFolder
        {
            get => APPLOCATION + $@"\{internalLogsFolder}\";
        }

        public static string GetImageCacheLocation(string appName = "")
        {
            string location = APPLOCATION + $@"\{imageCacheFolderName}\";
            if (appName != "") { location += $"{appName}.ico"; }
            return location;
        }
    }

}
