using System;
using System.Collections.Generic;
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
    }
}
