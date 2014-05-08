using System;
using System.Linq;
using System.Threading.Tasks;

using Orleans.Bus;

namespace Orleans.PingPong
{
    [Immutable, Serializable]
    public class Message
    {}

    [Immutable, Serializable]
    public class Initialize
    {
        public readonly string Destination;
        public readonly long Repeats;

        public Initialize(string destination, long repeats)
        {
            Destination = destination;
            Repeats = repeats;
        }
    }    
    
    [Immutable, Serializable]
    public class RunBenchmark
    {}

    [Immutable, Serializable]
    public class BenchmarkDone
    {
        public readonly long Pings;
        public readonly long Pongs;

        public BenchmarkDone(long pings, long pongs)
        {
            Pings = pings;
            Pongs = pongs;
        }
    }

    [Immutable, Serializable]
    public class Pong
    {
        public readonly string Sender;
        public readonly Message Payload;

        public Pong(string sender, Message payload)
        {
            Sender = sender;
            Payload = payload;
        }
    }

    [Handles(typeof(Initialize))]
    [Handles(typeof(RunBenchmark))]
    [Handles(typeof(Pong))]
    [Notifies(typeof(BenchmarkDone))]
    [ExtendedPrimaryKey]
    public interface IPingGrain : IObservableGrain
    {
        [Dispatcher] Task Handle(object cmd);
    }

    [Immutable, Serializable]
    public class Ping
    {
        public readonly string Sender;
        public readonly Message Payload;

        public Ping(string sender, Message payload)
        {
            Sender = sender;
            Payload = payload;
        }
    }

    [Handles(typeof(Ping))]
    [ExtendedPrimaryKey]
    public interface IPongGrain : IGrain
    {
        [Dispatcher] Task Handle(object message);
    }
}
