using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.Reflection;

namespace WinterRose.Serialization.Version2
{
    internal class DefaultDeserializer : IDeserializer
    {
        public SerializeOptions Options { get; set; }

        public event Action<ProgressLog> Logger = delegate { };
        public event Action<ProgressLog> AsyncLogger = delegate { };
        public event Action<ProgressLog> SimpleLogger = delegate { };
        public ParallelLoopState ParallelLoopState { get; set; }

        public bool Paused => paused;

        public bool Aborted => abort;

        private bool abort = false;
        private bool paused = false;

        public DefaultDeserializer(SerializeOptions options)
        {
            this.Options = options;
            Logger += (str) => _ = CommitLog(str);
        }

        public DeserializationResult Deserialize<T>(string data)
        {
            if (data[0] != Options.ObjectStart)
                throw new DeserializationFailedException($"No start of object found in the provided data. [{data[..15]}...]");

            int index = data.IndexOf(Options.ObjectEnd);
            if (index == -1)
                throw new DeserializationFailedException($"No end of object found in the provided data. [...{data[(data.Length - 15)..]}]");

            T obj = ActivatorExtra.CreateInstance<T>();
            ReflectionHelper<T> result = new ReflectionHelper<T>(ref obj);
            ParseObject(result, data[1..index]);
            return new(result.Value);
        }

        private List<string> GetObjectDefinitions(string data)
        {
            List<string> objectDefinitions = new List<string>();

            int startIndex = 0;
            int braceCount = 0;

            for (int i = 0; i < data.Length; i++)
            {
                if (abort)
                {
                    return [];
                }
                if(paused)
                {
                    SimpleLogger(new(0, 0, i, data.Length, $"Pausing..."));
                    while (paused) 
                    {
                    }
                    SimpleLogger(new(0, 0, i, data.Length, $"Resuming..."));
                }
                if (data[i] == '{')
                {
                    braceCount++;
                }
                else if (data[i] == '}')
                {
                    braceCount--;

                    if (braceCount == 0)
                    {
                        string section = data.Substring(startIndex, i - startIndex + 1);
                        objectDefinitions.Add(section);
                        startIndex = i + 1;
                    }
                }

                if(i % Options.SimpleLoggingInterval is 0)
                    SimpleLogger(new(0, 0, i, data.Length, $"Isolating object definitions..."));
            }

            return objectDefinitions;
        }

        public DeserializationResult DeserializeCollection<T>(string data)
        {
            SimpleLogger("Starting Deserialization...");
            ConcurrentBag<CollectionResult<T>> collection = new();
            if (abort)
            {
                return new("Aborted");
            }

            SimpleLogger("Isolating object definitions...");
            List<string> objectDefinitions = GetObjectDefinitions(data);
            var parts = objectDefinitions.Partition(Options.MaxThreads);

            var loopResult = Parallel.For(0, parts.Length, (i, state) =>
            {
                ParallelLoopState = state;
                int handled = 0;
                int timesLogged = 0;
                //int i = 0;
                List<CollectionResult<T>> temp = new();
                foreach (string item in parts[i])
                {
                    if(abort)
                    {
                        state.Break();
                        return;
                    }
                    if (paused)
                    {
                        SimpleLogger(new(0, 0, i, data.Length, $"Pausing..."));
                        while (paused)
                        {
                        }
                        SimpleLogger(new(0, 0, i, data.Length, $"Resuming..."));
                    }
                    var deserializeResult = Deserialize<T>(item);
                    temp.Add(new(deserializeResult.Result, i));
                    handled++;

                    if (handled % Options.SimpleLoggingInterval is 0)
                    {
                        timesLogged++;
                        SimpleLogger(new(i, handled, handled * timesLogged, parts[i].Count, $"Deserialized {handled * timesLogged} objects on thread {i}"));
                        handled = 0;
                    }
                    //i++;
                }
                int num = handled * timesLogged;
                int handlesleft = parts[i].Count - num;
                SimpleLogger(new(i, 0, parts[i].Count, parts[i].Count, $"thread {i} Finished."));
                temp.Reverse();
                temp.Foreach(x => collection.Add(x));
            });

            if (abort)
            {
                return new("Aborted");
            }

            List<T> finalResult = [];
            int cycles = 0;
            foreach (var item in collection.OrderBy(x => x.Index))
            {
                if (abort)
                    return new("Aborted");

                finalResult.Add(item.Result);
                cycles++;
                if(cycles % Options.SimpleLoggingInterval is 0)
                    SimpleLogger(new(0, Options.MaxThreads + 1, cycles, collection.Count, $"thread 0 is finishing up and added {cycles}/{collection.Count} items to the collection."));
            }

            SimpleLogger("Finihsed");
            return new(finalResult);
        }

        private void ParseObject<T>(ReflectionHelper<T> rh, string str)
        {
            string var;
            while (str.Length > 0)
            {
                int index = str.IndexOf(Options.FieldSeparator);
                if (index == -1)
                    break;
                var = str[..];
                ParseField(rh, var);
                str = str[(index + 1)..];
            }
        }

        private void ParseField<T>(ReflectionHelper<T> rh, string str)
        {
            int index = str.IndexOf(Options.NameValueSeparator);
            if (index == -1)
                throw new DeserializationFailedException($"No name value separator found in the provided data. [{str[..15]}...]");
            string name = str[..index];
            str = str[(index + 1)..];

            index = str.IndexOf(Options.FieldSeparator);
            if (index == -1)
                throw new DeserializationFailedException($"No field separator found in the provided data. [{str[..15]}...]");
            string value = str[..index];
            str = str[(index + 1)..];

            Logger($"Parsing {name}");
            object val = ParseValue(rh, name, value);

            rh.SetValue(name, val);
        }

        private object ParseValue<T>(ReflectionHelper<T> rh, string name, string value)
        {
            Type t = rh.GetTypeOf(name);
            return TypeWorker.CastPrimitive(value, t);
        }

        private async Task CommitLog(ProgressLog str)
        {
            await Task.Run(() => AsyncLogger(str));
        }

        public void Abort()
        {
            if (paused)
                Resume();

            abort = true;
        }

        public void Resume()
        {
            abort = false;
            paused = false;
        }

        public void Pause()
        {
            paused = true;
        }

        private record CollectionResult<T>(T Result, int Index);
    }
}
