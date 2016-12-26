using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Streams;
using NLog;
using Topshelf;

namespace StreamTcpServer
{
    class StreamTcpService : ServiceControl
    {
        private static string address = "192.168.74.1";
        private static int port = 666;
        private ExtendedActorSystem system;
        private Logger _logger = LogManager.GetCurrentClassLogger();
        public bool Start(
            HostControl hostControl)
        {
            system = (ExtendedActorSystem)ActorSystem.Create("StreamTcpServer");
            ActorMaterializer materializer = system.Materializer();

            TcpServer server = new TcpServer(address, port, materializer, system);
            server.Start();
            _logger.Info("Stream Tcp started successfully...");
            return true;
        }

        public bool Stop(
            HostControl hostControl)
        {
            system.Terminate().ContinueWith(task =>
            {
                if(task.IsCompleted && !task.IsCanceled)
                    _logger.Info("Stream Tcp stopped");
            });
            return true;
        }
    }
}
