using System;
using System.Collections.Generic;
using System.Text;

namespace Lib.Models
{
    public class AppDataPoint
    {
        public string Name { get; set; }
        public int Value { get; set; }

        public AppDataPoint(string name, int value)
        {
            this.Name = name;
            this.Value = value;
        }
    }
}
