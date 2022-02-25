using DigitalWellbeingWPF.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace DigitalWellbeingWPF.Models
{
    /*
     * When removing a tag, don't re-use the numbers already in here. Just comment them and use a new int for ID.
     */
    public enum AppTag
    {
        None = 0,
        Work = 1,
        Education = 2,
        Games = 3,
        Entertainment = 4,
    }

    public static class AppTagHelper
    {
        public static Dictionary<string, int> GetComboBoxChoices()
        {
            Dictionary<string, int> tags = new Dictionary<string, int>();

            foreach (AppTag tag in Enum.GetValues(typeof(AppTag)))
            {
                // Name: Enum
                tags.Add(Enum.GetName(typeof(AppTag), tag), (int)tag);
            }

            return tags;
        }

        public static AppTag GetAppTag(string processName)
        {
            try
            {
                return SettingsManager.appTags[processName];
            }
            catch
            {
                return AppTag.None;
            }
        }

        public static string GetTagName(AppTag appTag)
        {
            return Enum.GetName(typeof(AppTag), appTag);
        }
    }
}
