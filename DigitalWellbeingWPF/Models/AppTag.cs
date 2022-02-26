﻿using DigitalWellbeingWPF.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        public static Dictionary<int, string> AppTagColors = new Dictionary<int, string>()
        {
            {0, "#212121"}, // None, Gray
            {1, "#E65100"}, // Work, Orange
            {2, "#1B5E20"}, // Education, Green
            {3, "#4A148C"}, // Games, Purple
            {4, "#B71C1C"}, // Entertainment, Red
            {5, "#0D47A1"}, // Communication, Blue
            {6, "#F57F17"}, // Utility, Yellow
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
            BrushConverter bc = new BrushConverter();
            return (Brush)bc.ConvertFromString(AppTagColors[(int)appTag]);
        }

        public static Brush GetTagColor(string appTagName)
        {
            BrushConverter bc = new BrushConverter();

            return (Brush)bc.ConvertFromString(AppTagColors[(int)EnumUtils.GetEnumValueFromName<AppTag>(appTagName)]);
        }
    }
}
