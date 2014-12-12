using EP;
using EP.Text;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PullEntiCLI
{
    public static class MainClass
    {
        private static object _syncRoot = new object();
        private static Dictionary<int, Worker> workers = new Dictionary<int, Worker>();

        public static void Main(string[] args)
        {
            Console.Clear();
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

            Run(input: input, outputBase: output);
        }

        /// <summary>
        /// Processes json items from input and wtores it into output.
        /// Takes only data in json, formatted like {title: '', content: ''},
        /// and written in UTF8-encoded file, where every entity takes separate line.
        /// </summary>
        /// <param name="input">Input file.</param>
        /// <param name="outputBase">Output directory name.</param>
        private static void Run(String input, String outputBase = "")
        {
            var _coll = new BlockingCollection<string>();
            var tasks = new List<Task<Task>>();
            var startPos = 0;
            var taskStartPos = 1;

            Task.Factory.StartNew(() =>
            {
                using (var file = new StreamReader(input, Encoding.UTF8))
                {
                    WriteConsole("Started reading", startPos);

                    while (!file.EndOfStream)
                        _coll.Add(file.ReadLine());

                    _coll.CompleteAdding();
                }
            });

            for (int i = 1; i < Environment.ProcessorCount + 1; i++)
            {
                var id = i;

                tasks.Add(Task.Factory.StartNew(async () =>
                {
                    var cnt = 0;
                    var worker = new Worker();
                    var output = outputBase + Path.DirectorySeparatorChar + id + ".json";

                    using (var outer = new StreamWriter(path: output, append: true))
                        foreach (var line in _coll.GetConsumingEnumerable())
                        {
                            var item = JsonConvert.DeserializeObject<Item>(line);
                            outer.WriteLine(JsonConvert.SerializeObject(worker.ProcessArticle(item)));

                            WriteConsole(string.Format("Task {0}: {1,6}", id, ++cnt), id + taskStartPos);
                        }
                }));
            }

            Task.WaitAll(tasks.ToArray());

            Console.Clear();
            Console.WriteLine("Done");
        }

        /// <summary>
        /// Synchronised console output
        /// </summary>
        /// <param name="msg">Message</param>
        /// <param name="top">Offset from the top in lines</param>
        public static void WriteConsole(string msg, int top)
        {
            lock (_syncRoot)
            {
                Console.SetCursorPosition(0, top);
                Console.WriteLine(msg);
                Console.SetCursorPosition(0, 0);
            }
        }
    }
}