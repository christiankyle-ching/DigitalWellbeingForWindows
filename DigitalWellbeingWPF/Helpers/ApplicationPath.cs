using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Environment;

namespace DigitalWellbeingWPF.Helpers
{
    public static class ApplicationPath
    {
        static readonly SpecialFolder applicationPath = SpecialFolder.LocalApplicationData;
        static readonly string applicationFolderName = "digital-wellbeing";
        static readonly string imageCacheFolderName = "processicons";
        static readonly string dailyLogsFolderName = "dailylogs";
        static readonly string internalLogsFolder = "internal-logs";
        static readonly string settingsFolder = "settings";

        private static string GetApplicationLocation
        {
            get => GetFolderPath(applicationPath) + $@"\{applicationFolderName}";
        }

        public static string UsageLogsFolder
        {
            get => GetApplicationLocation + $@"\{dailyLogsFolderName}\";
        }

        public static string SettingsFolder
        {
            get => GetApplicationLocation + $@"\{settingsFolder}\";
        }

        public static string InternalLogsFolder
        {
            get => GetApplicationLocation + $@"\{internalLogsFolder}\";
        }

        public static string GetImageCacheLocation(string appName = "")
        {
            string location = GetApplicationLocation + $@"\{imageCacheFolderName}\";
            if (appName != "") { location += $"{appName}.ico"; }
            return location;
        }
    }
}
