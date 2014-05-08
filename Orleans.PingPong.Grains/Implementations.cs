using System;
using System.Linq;
using System.Threading.Tasks;

using Orleans.Bus;

namespace Orleans.PingPong
{
    public class PingGrain : MessageBasedGrain, IPingGrain, IObservableGrain
    {
        readonly IObserverCollection observers = new ObserverCollection();

        public Task Attach(Observes o, Type e)
        {
            observers.Attach(o, e);
            return TaskDone.Done;
        }

        public Task Detach(Observes o, Type e)
        {
            observers.Detach(o, e);
            return TaskDone.Done;
        }

        public Task Handle(object cmd)
        {
            return Handle((dynamic) cmd);
        }

        static readonly Message msg = new Message();

        string destination;
        long pings;
        long pongs;
        long repeats;

        public Task Handle(Initialize cmd)
        {
            destination = cmd.Destination;
            repeats = cmd.Repeats;
            
            return TaskDone.Done;
        }

        public Task Handle(RunBenchmark cmd)
        {
            Bus.Send(destination, new Ping(this.Id(), msg)).Ignore();
            pings++;

            return TaskDone.Done;
        }

        public Task Handle(Pong cmd)
        {
            pongs++;

            if (pings < repeats)
            {
                Bus.Send(destination, new Ping(this.Id(), msg)).Ignore();
                pings++;
            }
            else if (pongs >= repeats)
            {
                observers.Notify(this.Id(), new BenchmarkDone(pings, pongs));
            }

            return TaskDone.Done;
        }
    }

    public class PongGrain : MessageBasedGrain, IPongGrain
    {
        public Task Handle(object cmd)
        {
            return Handle((dynamic)cmd);
        }

        public Task Handle(Ping cmd)
        {
            Bus.Send(cmd.Sender, new Pong(this.Id(), cmd.Payload)).Ignore();
            return TaskDone.Done;
        }
    }
}
