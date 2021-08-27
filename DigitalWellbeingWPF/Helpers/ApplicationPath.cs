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
        public static string LogsFolder
        {
            get => Environment.ExpandEnvironmentVariables(@"%USERPROFILE%\.digitalwellbeing\dailylogs\");
        }

        static readonly SpecialFolder applicationPath = SpecialFolder.LocalApplicationData;
        static readonly string applicationFolderName = "digital-wellbeing";
        static readonly string imageCacheFolderName = "processicons";

        private static string GetApplicationLocation
        {
            get => GetFolderPath(applicationPath) + $@"\{applicationFolderName}";
        }

        public static string GetImageCacheLocation(string appName = "")
        {
            string location = GetApplicationLocation + $@"\{imageCacheFolderName}\";
            if (appName != "") { location += $"{appName}.ico"; }
            return location;
        }
    }
}
