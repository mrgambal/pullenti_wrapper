using EP;
using EP.Text;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Collections.Generic;

namespace PullEntiCLI
{

    public static class MainClass
    {
        private static Dictionary<int, Worker> workers = new Dictionary<int, Worker>();

        public static void Main(string[] args)
        {
            ProcessorService.Initialize(true, MorphLang.UA);

            var input = args[0];

            if (input.StartsWith(@"~", StringComparison.Ordinal))
                input = Environment.SpecialFolder.Personal + input.TrimStart(new[] { '~' });

            if (!File.Exists(input))
            {
                Console.WriteLine(String.Format("File not found {0}", input));
                Environment.Exit(1);
            }

            var output = Path.GetDirectoryName(input);

            RunParallel(input: input, outputBase: output);
        }

        /// <summary>
        /// Processes json items from input and wtores it into output.
        /// Takes only data in json, formatted like {title: '', content: ''},
        /// and written in UTF8-encoded file, where every entity takes separate line.
        /// </summary>
        /// <param name="input">Input file.</param>
        /// <param name="outputBase">Output directory name.</param>
        private static void RunParallel(String input, String outputBase = "")
        {
            string line;
            var counter = 0;
            var sum = 0;
            var maxThreads = Environment.ProcessorCount;
            var resetEvents = GetResetEvents(maxThreads);

            ThreadPool.SetMaxThreads(maxThreads, maxThreads);

            using (var file = new StreamReader(input, Encoding.UTF8))
            {
                while (!file.EndOfStream)
                {
                    line = file.ReadLine();
                    resetEvents[counter].Reset();

                    ThreadPool.QueueUserWorkItem(state =>
                    {
                        var worker = GetWorker(Thread.CurrentThread.GetHashCode());
                        var payload = (Payload)state;
                        var item = JsonConvert.DeserializeObject<Item>(payload.Line);
                        var output = outputBase
                                     + Path.DirectorySeparatorChar
                                     + payload.Counter
                                     + ".json";

                        using (var outer = new StreamWriter(path: output, append: true))
                            outer.WriteLine(JsonConvert.SerializeObject(worker.ProcessArticle(item)));

                        resetEvents[payload.Counter].Set();
                    }, new Payload { Counter = counter, Line = line });

                    if (counter == maxThreads - 1 || file.EndOfStream)
                    {
                        WaitHandle.WaitAll(resetEvents);
                        sum += maxThreads;

                        if (sum % 1000 == 0)
                            Console.WriteLine("I've read " + sum + " articles, sir!");
                    }

                    counter = (counter + 1) % maxThreads;
                }
            }

            Console.WriteLine("Oi, oi, captain! There were " + sum + " fecking articles");
        }

        /// <summary>
        /// Static workers pool.
        /// </summary>
        /// <returns>Worker instance.</returns>
        /// <param name="key">Thread hash-code</param>
        private static Worker GetWorker(int key)
        {
            if (!workers.ContainsKey(key))
                lock(workers)
                    workers[key] = new Worker();

            return workers[key];
        }

        /// <summary>
        /// Fills in and returns array of events
        /// </summary>
        /// <param name="quantity">Array-size. Should be equal to your MaxThreads</param>
        /// <returns>Array of ManualResetEvents</returns>
        private static ManualResetEvent[] GetResetEvents(int quantity)
        {
            var resetEvents = new ManualResetEvent[quantity];

            for (var i = 0; i < quantity; i++)
                resetEvents[i] = new ManualResetEvent(false);

            return resetEvents;
        }
    }
}
