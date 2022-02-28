using DigitalWellbeingWPF.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace DigitalWellbeingWPF.Models
{
    /*
     * When removing a tag, don't re-use the numbers already in here.
     * Just comment them and use a new int for ID.
     * You can rename the enum string, though keep in mind that this might confuse you for previous tagged apps.
     * Underscores (_) can be replaced by spaces ( ). See: `EnumUtils.GetEnumDisplayName`
     */
    public enum AppTag
    {
        Untagged = 0,
        Work = 1,
        Education = 2,
        Games = 3,
        Entertainment = 4,
        Communication = 5,
        Utility = 6,
    }

    public static class AppTagHelper
    {
        /*
         * Match int ID from AppTag enum
         * 
         * Colors From 2014 Material Design Palette
         * Color[900] for dark colors, since foreground is always white
         * https://material.io/design/color/the-color-system.html#tools-for-picking-colors
         */
        public static Dictionary<int, Brush> AppTagColors = new Dictionary<int, Brush>()
        {
            {0, (Brush)Application.Current.TryFindResource("Material_Gray") ?? Brushes.Gray}, // None, Gray,
            {1, (Brush)Application.Current.TryFindResource("Material_Orange") ?? Brushes.Orange}, // Work, Orange,
            {2, (Brush)Application.Current.TryFindResource("Material_Green") ?? Brushes.Green}, // Education, Green
            {3, (Brush)Application.Current.TryFindResource("Material_Purple") ?? Brushes.Purple}, // Games, Purple
            {4, (Brush)Application.Current.TryFindResource("Material_Red") ?? Brushes.Red}, // Entertainment, Red
            {5, (Brush)Application.Current.TryFindResource("Material_Blue") ?? Brushes.Blue}, // Communication, Blue
            {6, (Brush)Application.Current.TryFindResource("Material_Yellow") ?? Brushes.Yellow}, // Utility, Yellow
        };

        public static Dictionary<string, int> GetComboBoxChoices()
        {
            Dictionary<string, int> tags = new Dictionary<string, int>();

            foreach (AppTag tag in Enum.GetValues(typeof(AppTag)))
            {
                // Name: Enum
                tags.Add(EnumUtils.GetEnumName(tag), (int)tag);
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
                return AppTag.Untagged;
            }
        }

        public static string GetTagDisplayName(AppTag appTag)
        {
            if (appTag == AppTag.Untagged) return "";

            return EnumUtils.GetEnumName(appTag);
        }

        public static Brush GetTagColor(AppTag appTag)
        {
            return AppTagColors[(int)appTag];
        }

        public static Brush GetTagColor(string appTagName)
        {
            AppTag _tag = EnumUtils.GetEnumValueFromName<AppTag>(appTagName);
            return AppTagColors[(int)_tag];
        }
    }
}
