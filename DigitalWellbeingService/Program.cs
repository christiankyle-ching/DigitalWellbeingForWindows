using System;
using System.Diagnostics;
using Topshelf;

namespace DigitalWellbeingService
{
    class Program
    {
        static void Main(string[] args)
        {
            var rc = HostFactory.Run(host =>
            {
                host.Service<ActivityLogger>(service =>
                {
                    service.ConstructUsing(name => new ActivityLogger());
                    service.WhenStarted(al => al.Start());
                    service.WhenStopped(al => al.Stop());
                    service.WhenShutdown(al => al.Shutdown());
                });

                host.EnableShutdown();
                host.EnableSessionChanged();
                host.StartAutomatically();

                host.SetServiceName("DigitalWellbeing");
                host.SetDisplayName("Digital Wellbeing");
                host.SetDescription("This is a required service for monitoring your app usage.");

                host.RunAsLocalSystem();
            });

            var exitCode = (int)Convert.ChangeType(rc, rc.GetTypeCode());
            Environment.ExitCode = exitCode;
        }
    }
}
