using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DigitalWellbeingService.NET4._6
{
    class Program
    {
        private static int checkInterval = 1000;

        static void Main(string[] args)
        {
            ActivityLogger _al = new ActivityLogger();

            while (true)
            {
                _al.OnTimer();
                Thread.Sleep(checkInterval);
            }
        }
    }
}
