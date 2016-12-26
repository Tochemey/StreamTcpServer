using System.Text;
using Akka;
using Akka.Actor;
using Akka.IO;
using Akka.Streams;
using Akka.Streams.Dsl;
using Akka.Streams.Stage;
using NLog;
using Tcp = Akka.Streams.Dsl.Tcp;

namespace AdvancedStreamTcpServer
{
    public class AdvancedTcpServer
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        public string Address { get; }
        public int Port { get; }
        public ActorMaterializer Materializer { get; }
        public ExtendedActorSystem System { get; }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        protected GraphStage<FlowShape<string, string>> CloseConnection()
        {
            var inlet = new Inlet<string>("CloseConnection.In");
            var outlet = new Outlet<string>("CloseConnection.Out");
            return new CloseConnectionGraphStage(inlet,
                outlet);
        }

        protected Flow<ByteString, ByteString, NotUsed> Handle(
            Tcp.IncomingConnection connection)
        {
            var graph = Flow.FromGraph(GraphDsl.Create(builder =>
            {
                var welcome =
                    Source.Single(
                        ByteString.FromString($"Welcome port {connection.RemoteAddress}"));

                var logic = builder.Add(Flow.Create<ByteString>()
                    .Via(Framing.Delimiter(ByteString.FromString("\n"),
                        256,
                        true))
                    .Via(
                        Flow.Create<ByteString>()
                            .Select(bytes =>
                            {
                                var message = Encoding.UTF8.GetString(bytes.ToArray());
                                _logger.Info($"received {message}");
                                return message;
                            })
                    )
                    .Via(CloseConnection())
                    .Via(Flow.Create<string>()
                        .Select(s =>
                        {
                            var message = $"Server hereby responds to message: {s}\n";
                            return ByteString.FromString(message);
                        }))
                    );

                var concat = builder.Add(Concat.Create<ByteString>());
                var in0 = concat.In(0);

               // welcome.Via(in0);
                

                return new FlowShape<ByteString, ByteString>(logic.Inlet,
                    concat.Out);
            }));
            return graph;
        }

        private sealed class CloseConnectionGraphStage :
            GraphStage<FlowShape<string, string>>
        {
            public CloseConnectionGraphStage(
                Inlet<string> inlet,
                Outlet<string> outlet)
            {
                Inlet = inlet;
                Outlet = outlet;
                Shape = new FlowShape<string, string>(inlet,
                    outlet);
            }

            public override FlowShape<string, string> Shape { get; }
            private Inlet<string> Inlet { get; }
            private Outlet<string> Outlet { get; }

            protected override GraphStageLogic CreateLogic(
                Attributes inheritedAttributes) => new CloseConnectionStageLogic(Shape,
                    Inlet,
                    Outlet);
        }

        private sealed class CloseConnectionStageLogic : GraphStageLogic
        {
            public CloseConnectionStageLogic(
                int inCount,
                int outCount,
                Inlet<string> inlet,
                Outlet<string> outlet) : base(inCount,
                    outCount)
            {
                Inlet = inlet;
                Outlet = outlet;
            }

            public CloseConnectionStageLogic(
                Shape shape,
                Inlet<string> inlet,
                Outlet<string> outlet) : base(shape)
            {
                Inlet = inlet;
                Outlet = outlet;
                SetHandler(inlet,
                    OnPush);
                SetHandler(outlet,
                    OnPull);
            }

            private Inlet<string> Inlet { get; }
            private Outlet<string> Outlet { get; }

            private void OnPull() => Pull(Inlet);

            private void OnPush()
            {
                var grabbed = Grab(Inlet);
                switch (grabbed)
                {
                    case "q":
                        Push(Outlet,
                            "BYE");
                        CompleteStage();
                        break;
                    default:
                        Push(Outlet,
                            $"Server hereby responds to message: {grabbed}\n");
                        break;
                }
            }
        }
    }
}