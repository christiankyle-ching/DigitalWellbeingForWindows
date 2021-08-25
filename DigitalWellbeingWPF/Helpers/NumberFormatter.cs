using ModernWpf.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalWellbeingWPF.Helpers
{
    public class NumberFormatter
    {
        public class WholeNumberFormatter : INumberBoxNumberFormatter
        {
            public string FormatDouble(double value)
            {
                return string.Format("{0}", value);
            }

            public double? ParseDouble(string text)
            {
                if (double.TryParse(text, out double result))
                {
                    return Math.Round(result, 0);
                }
                return null;
            }
        }
    }
}
