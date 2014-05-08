using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management;
using System.Threading.Tasks;

using Orleans.Bus;

namespace Orleans.PingPong
{
    public class Benchmark
    {
        const string resultsFile = @"C:\Temp\OrleansPingPong.txt";

        readonly List<string> clients = new List<string>();
        readonly List<BenchmarkDoneAwaiter> awaiters = new List<BenchmarkDoneAwaiter>();

        readonly int numberOfClients;
        readonly int numberOfRepeatsPerClient;

        readonly IMessageBus bus = MessageBus.Instance;
        readonly ISubscriptionManager subscriptions = SubscriptionManager.Instance;

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
                var client = "C" + i;
                var destination = "D" + i;
                clients.Add(client);

                await bus.Send(client, 
                  new Initialize(destination, numberOfRepeatsPerClient));

                var awaiter = new BenchmarkDoneAwaiter();
                awaiters.Add(awaiter); // to prevent GC collection of observer

                var observer = await subscriptions.CreateObserver(awaiter);
                await subscriptions.Subscribe<BenchmarkDone>(client, observer);
                
                results.Add(awaiter.AsTask());
            }

            clients.ForEach(id => bus.Send(id, new RunBenchmark()));

            var stopwatch = Stopwatch.StartNew();
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
            var totalMessagesReceived = numberOfRepeatsPerClient * totalActorCount * 2; // communication in Orleans' is always two-way
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
   
    public class BenchmarkDoneAwaiter : Observes
    {
        readonly TaskCompletionSource<ClientResult> tcs = new TaskCompletionSource<ClientResult>();

        public Task<ClientResult> AsTask()
        {
            return tcs.Task;
        }

        public void On(string source, object e)
        {
            var done = (BenchmarkDone) e;
            tcs.SetResult(new ClientResult(done.Pings, done.Pongs));
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
