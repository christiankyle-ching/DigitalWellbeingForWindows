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
        Communication = 5,
    }

    public static class AppTagHelper
    {
        /*
         * Match int ID from AppTag enum
         */
        public static Dictionary<int, string> AppTagColors = new Dictionary<int, string>()
        {
            {0, "#9E9E9E"},
            {1, "#FF9800"},
            {2, "#4CAF50"},
            {3, "#9C27B0"},
            {4, "#F44336"},
            {5, "#00BCD4"},
        };

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
            if (appTag == AppTag.None) return "";

            return Enum.GetName(typeof(AppTag), appTag);
        }

        public static Brush GetTagColor(AppTag appTag)
        {
            BrushConverter bc = new BrushConverter();
            return (Brush)bc.ConvertFromString(AppTagColors[(int)appTag]);
        }
    }
}
