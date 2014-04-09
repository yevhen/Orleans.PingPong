using System;
using System.Linq;
using System.Threading.Tasks;

namespace Orleans.PingPong
{
    public class Client : GrainBase, IClient
    {
        static readonly Message msg = new Message();

        IDestination actor;
        ObserverSubscriptionManager<IClientObserver> subscribers;

        long pings;
        long pongs;
        long repeats;

        public override Task ActivateAsync()
        {
            subscribers = new ObserverSubscriptionManager<IClientObserver>();
            return TaskDone.Done;
        }

        public Task Initialize(IDestination actor, long repeats)
        {
            this.actor = actor;
            this.repeats = repeats;

            return TaskDone.Done;
        }

        public Task Run()
        {
            actor.Ping(this, msg);
            pings++;

            return TaskDone.Done;
        }

        public Task Pong(IDestination @from, Message message)
        {
            pongs++;

            if (pings < repeats)
            {
                actor.Ping(this, msg);
                pings++;
            }
            else if (pongs >= repeats)
            {
                subscribers.Notify(x => x.Done(pings, pongs));
            }

            return TaskDone.Done;
        }

        public Task Subscribe(IClientObserver subscriber)
        {
            subscribers.Subscribe(subscriber);
            return TaskDone.Done;
        }
    }

    public class Destination : GrainBase, IDestination
    {
        public Task Ping(IClient @from, Message message)
        {
            from.Pong(this, message);
            return TaskDone.Done;
        }
    }
}
