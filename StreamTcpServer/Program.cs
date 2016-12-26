using System.Diagnostics;
using Topshelf;

namespace StreamTcpServer
{
    internal class Program
    {
        private static int Main()
        {
            var serviceName = "StreamTcpService";
            var displayName = serviceName;
            var description = serviceName;
            return (int) HostFactory.Run(x =>
            {
                x.SetServiceName(serviceName);
                x.SetDisplayName(displayName);
                x.SetDescription(description);

                x.UseAssemblyInfoForServiceInfo();
                x.RunAsLocalSystem();
                x.StartAutomatically();
                x.UseNLog();
                x.Service<StreamTcpService>();
                x.EnableServiceRecovery(r => r.RestartService(1));
                x.DependsOnEventLog();
                x.EnableShutdown();
                x.OnException(exception =>
                {
                    EventLog.WriteEntry(serviceName,
                        exception.StackTrace,
                        EventLogEntryType.Error);
                });
            });
        }
    }
}