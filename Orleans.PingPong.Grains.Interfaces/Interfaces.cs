using System;
using System.Linq;
using System.Threading.Tasks;

namespace Orleans.PingPong
{
    [Immutable]
    public class Message
    {}

    public interface IClient : IGrain
    {
        Task Run();
        Task Pong(IDestination from, Message message);
        Task Initialize(IDestination actor, long repeats);
        Task Subscribe(IClientObserver subscriber);
    }

    public interface IClientObserver : IGrainObserver
    {
        void Done(long pings, long pongs);
    }

    public interface IDestination : IGrain
    {
        Task Ping(IClient from, Message message);
    }
}
