using System;
using System.Text;
using System.Threading.Tasks;
using Akka;
using Akka.Actor;
using Akka.IO;
using Akka.Streams;
using Akka.Streams.Dsl;
using NLog;
using Tcp = Akka.Streams.Dsl.Tcp;
using TcpExt = Akka.Streams.Dsl.TcpExt;

namespace StreamTcpServer
{
    public class TcpServer
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        public TcpServer(
            string address,
            int port,
            ActorMaterializer materializer,
            ExtendedActorSystem system)
        {
            Address = address;
            Port = port;
            Materializer = materializer;
            System = system;

            Connections = new TcpExt(System).Bind(Address,
                Port);
        }

        public string Address { get; }
        public int Port { get; }
        public ActorMaterializer Materializer { get; }
        public ExtendedActorSystem System { get; }

        public Source<Tcp.IncomingConnection, Task<Tcp.ServerBinding>> Connections { get;
        }

        public void Start() => Connections.To(Sink.ForEach<Tcp.IncomingConnection>(
            connection =>
            {
                _logger.Info(
                    $"incoming connection from {connection.RemoteAddress}");
                connection.HandleWith(Handle(),
                    Materializer);
            }))
            .Run(Materializer)
            .ContinueWith(task => _logger.Info(
                task.IsCompleted && !task.IsFaulted
                    ? $"Server started, listening on: ${task.Result.LocalAddress}"
                    : "failed to start"));


        private static Flow<ByteString, ByteString, NotUsed> Handle()
        {
            var eol = ByteString.FromString("\n");
            var delimiter = Framing.Delimiter(eol,
                256,
                true);

            var receiver = Flow.Create<ByteString>()
                .Select(bytes =>
                {
                    var message = Encoding.UTF8.GetString(bytes.ToArray());
                    _logger.Info($"received {message}");
                    return message;
                });

            var responder = Flow.Create<string>()
                .Select(s =>
                {
                    var message = $"Server hereby responds to message: {s}\n";
                    return ByteString.FromString(message);
                });

            return Flow.Create<ByteString>()
                .Via(delimiter.Async())
                .Via(receiver.Async())
                .Via(responder.Async());
        }
    }
}