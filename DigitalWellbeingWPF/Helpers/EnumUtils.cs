using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalWellbeingWPF.Helpers
{
    public static class EnumUtils
    {
        public static string GetEnumName<T>(T _enum)
        {
            try
            {
                return Enum.GetName(typeof(T), _enum);
            }
            catch
            {
                return "";
            }
        }

        public static string GetEnumDisplayName<T>(T _enum)
        {
            return GetEnumName(_enum).Replace('_', ' ');
        }

        public static T GetEnumValueFromName<T>(string _name)
        {
            try
            {
                return (T)Enum.Parse(typeof(T), _name);
            }
            catch
            {
                throw;
            }
        }
    }
}
