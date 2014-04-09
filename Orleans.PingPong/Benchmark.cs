using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Threading.Tasks;

namespace Orleans.PingPong
{
    public class Benchmark
    {
        const string resultsFile = @"C:\Temp\OrleansPingPong.txt";
        
        readonly List<IClient> clients = new List<IClient>();
        readonly List<IClientObserver> observers = new List<IClientObserver>();

        readonly int numberOfClients;
        readonly int numberOfRepeatsPerClient;

        public Benchmark(int numberOfClients, int numberOfRepeatsPerClient)
        {
            this.numberOfClients = numberOfClients;
            this.numberOfRepeatsPerClient = numberOfRepeatsPerClient;
        }

        public async void Run()
        {
            var results = new List<Task<ClientResult>>();

            for (var i = 0; i < numberOfClients; i++)
            {
                var destination = DestinationFactory.GetGrain(Guid.NewGuid());

                var client = ClientFactory.GetGrain(Guid.NewGuid());
                await client.Initialize(destination, numberOfRepeatsPerClient);

                var observer = new ClientObserver();
                await client.Subscribe(ClientObserverFactory.CreateObjectReference(observer).Result);
                
                clients.Add(client);
                observers.Add(observer); // to prevent GC collection of observer
                results.Add(observer.AsTask());
            }

            var stopwatch = Stopwatch.StartNew();
            
            clients.ForEach(c => c.Run());
            await Task.WhenAll(results.ToArray());

            stopwatch.Stop();

            WriteResultsToConsole(stopwatch);
            WriteResultsToFile(stopwatch);
            
            Console.WriteLine();
            Console.WriteLine("Done!. Press any key to exit ...");
        }

        void WriteResultsToConsole(Stopwatch stopwatch)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            WriteResults(stopwatch, Console.Out);
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        void WriteResultsToFile(Stopwatch stopwatch)
        {
            using (var writer = File.AppendText(resultsFile))
                WriteResults(stopwatch, writer);
        }

        void WriteResults(Stopwatch stopwatch, TextWriter writer)
        {
            var totalActorCount = numberOfClients * 2L; // ping and pong actor
            var totalMessagesReceived = numberOfRepeatsPerClient * totalActorCount;
            var throughput = (totalMessagesReceived / stopwatch.Elapsed.TotalSeconds);

            writer.WriteLine("OSVersion: {0}", Environment.OSVersion);
            writer.WriteLine("ProcessorCount: {0}", Environment.ProcessorCount);
            writer.WriteLine("ClockSpeed: {0} MHZ", CpuSpeed());
            writer.WriteLine();
            writer.WriteLine("Actor Count: {0}", totalActorCount);
            writer.WriteLine("Total: {0:N} messages", totalMessagesReceived);
            writer.WriteLine("Time: {0:F} sec", stopwatch.Elapsed.TotalSeconds);
            writer.WriteLine("TPS: {0:##,###} per/sec", throughput);
        }

        static uint CpuSpeed()
        {
            using (var mo = new ManagementObject("Win32_Processor.DeviceID='CPU0'"))
                return (uint)(mo["CurrentClockSpeed"]);
        }
    }
   
    public class ClientObserver : IClientObserver
    {
        readonly TaskCompletionSource<ClientResult> tcs = new TaskCompletionSource<ClientResult>();

        public void Done(long pings, long pongs)
        {
            tcs.SetResult(new ClientResult(pings, pongs));
        }

        public Task<ClientResult> AsTask()
        {
            return tcs.Task;
        }
    }

    public class ClientResult
    {
        public readonly long Pings;
        public readonly long Pongs;

        public ClientResult(long pings, long pongs)
        {
            Pings = pings;
            Pongs = pongs;
        }
    }
}
