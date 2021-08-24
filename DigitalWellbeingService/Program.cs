using System;
using System.Diagnostics;
using System.Threading;

namespace DigitalWellbeingService
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
