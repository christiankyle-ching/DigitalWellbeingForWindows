using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using static System.Environment;

namespace DigitalWellbeingWPF.Helpers
{
    public static class IconManager
    {
        static readonly SpecialFolder applicationPath = SpecialFolder.LocalApplicationData;

        static readonly string applicationFolderName = "digital-wellbeing";
        static readonly string imageCacheFolderName = "processicons";

        public static BitmapSource GetIconSource(string appName)
        {
            CreateAppDirectories();

            BitmapSource cachedImage = GetCachedImage(appName);
            if (cachedImage != null) return cachedImage;

            Process[] processes = Process.GetProcessesByName(appName);

            if (processes.Length > 0)
            {
                try
                {
                    Icon icon = Icon.ExtractAssociatedIcon(processes[0].MainModule.FileName);

                    CacheImageLocation(icon, appName);
                    return Imaging.CreateBitmapSourceFromHIcon(icon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                }
                catch (Win32Exception)
                {
                    Debug.WriteLine($"Not enough permissions. Run as Administrator.");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"ICON - NOT FOUND: {ex}");
                }

            }

            return null;
        }

        private static void CreateAppDirectories()
        {
            Directory.CreateDirectory(GetImageCacheLocation(null));
        }

        private static string GetApplicationLocation()
        {
            return GetFolderPath(applicationPath) + $@"\{applicationFolderName}";
        }

        private static string GetImageCacheLocation(string appName)
        {
            string location = GetApplicationLocation() + $@"\{imageCacheFolderName}\";
            if (!(appName == "" || appName == null)) { location += $"{appName}.ico"; }
            return location;
        }

        private static void CacheImageLocation(Icon icon, string appName)
        {
            try
            {
                FileStream outputStream = new FileStream(GetImageCacheLocation(appName), FileMode.Create);
                icon.Save(outputStream);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"CACHE - FAILED: {ex}");
            }
        }

        private static BitmapImage GetCachedImage(string appName)
        {
            try
            {
                BitmapImage img = new BitmapImage();
                img.BeginInit();
                img.UriSource = new Uri(GetImageCacheLocation(appName));
                img.EndInit();

                return img;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"CACHE - NOT FOUND: {ex}");
            }

            return null;
        }
    }
}
