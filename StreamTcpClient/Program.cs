using System;
using System.Text;
using Akka.Actor;
using Akka.IO;
using Akka.Streams;
using Akka.Streams.Dsl;
using NLog;
using TcpExt = Akka.Streams.Dsl.TcpExt;

namespace StreamTcpClient
{
    internal class Program
    {
        private static readonly string address = "127.0.0.1";
        private static readonly int port = 666;
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        private static void Main(
            string[] args)
        {
            var system = (ExtendedActorSystem) ActorSystem.Create("StreamTcpClient");
            var materializer = system.Materializer();
            var connection = new TcpExt(system).OutgoingConnection(address,
                port);

            var flow = Flow.Create<ByteString>()
                .Via(Framing.Delimiter(ByteString.FromString("\n"),
                    256,
                    true))
                .Via(Flow.Create<ByteString>()
                    .Select(bytes =>
                    {
                        var message = Encoding.UTF8.GetString(bytes.ToArray());
                        _logger.Info(message);
                        return message;
                    }))
                .Via(Flow.Create<string>()
                    .Select(s => "Hello World"))
                .Via(Flow.Create<string>()
                    .Select(s => s += "\n"))
                .Via(Flow.Create<string>()
                    .Select(ByteString.FromString));

            connection.Join(flow)
                .Run(materializer);

            Console.ReadKey();
        }
    }
}