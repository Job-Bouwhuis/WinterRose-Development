using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using WinterRose.Encryption;
using WinterRose.Reflection;

namespace WinterRose.Serialization.Version2
{
    /// <summary>
    /// The default Serializer used by the <see cref="SnowSerializer"/> class.
    /// </summary>
    public class DefaultSerializer : ISerializer
    {
        public SerializeOptions Options { get; set; }

        public event Action<ProgressLog> Logger = delegate { };
        public event Action<ProgressLog> AsyncLogger = delegate { };
        public event Action<ProgressLog> SimpleLogger = delegate { };
        public ParallelLoopState ParallelLoopState => loopState;

        public bool Paused => paused;

        public bool Aborted => abort;

        private ParallelLoopState loopState;
        private bool abort = false;
        private bool paused = false;

        /// <summary>
        /// Creates a new instance of the <see cref="DefaultSerializer"/> class.
        /// </summary>
        /// <param name="options">The options to use in this serialization</param>
        public DefaultSerializer(SerializeOptions options)
        {
            Options = options;
            Logger += (str) => _ = CommitLog(str);
        }

        private async Task CommitLog(ProgressLog str)
        {
            await Task.Run(() => AsyncLogger(str));
        }

        public SerializationResult Serialize<T>(T obj)
        {
            abort = false;
            StringBuilder builder = new();
            try
            {
                builder.Append(Options.ObjectStart);
                AppendFieldsAndProperties(obj, builder);
                builder.Append(Options.ObjectEnd);

                return new(builder);
            }
            catch (SerializationFailedException e)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new SerializationFailedException("Failed to serialize object.", e);
            }
        }

        public SerializationResult SerializeCollection<T>(IEnumerable<T> collection)
        {
            SimpleLogger("Starting serialization of collection");
            ConcurrentBag<CollectionResult> results = new();

            List<T>[] parts = collection.Partition(Options.MaxThreads);

            Parallel.For(0, parts.Length, (i, state) =>
            {
                loopState = state;
                ConcurrentBag<CollectionResult> temp = new();
                int handled = 0;
                int timesLogged = 0;
                //int i = 0;
                foreach (T item in parts[i])
                {
                    if (abort)
                    {
                        state.Break();
                        return;
                    }
                    if (paused)
                    {
                        SimpleLogger(new(0, 0, i, temp.Count, $"Pausing..."));
                        while (paused)
                        {
                        }
                        SimpleLogger(new(0, 0, i, temp.Count, $"Resuming..."));
                    }
                    var serializeResult = Serialize(item);
                    temp.Add(new(serializeResult.ResultRaw, i));
                    handled++;

                    if (handled % Options.SimpleLoggingInterval is 0)
                    {
                        timesLogged++;
                        SimpleLogger(new(i, handled, handled * timesLogged, parts[i].Count, $"Serialized {handled * timesLogged} items on thead {i}"));
                        handled = 0;
                    }
                    //i++;
                }

                int num = handled * timesLogged;
                int handlesleft = parts[i].Count - num;

                int cycles = 0;
                temp.Foreach(x =>
                {
                    if (abort)
                    {
                        state.Break();
                        return;
                    }
                    if (paused)
                    {
                        SimpleLogger(new(0, 0, i, temp.Count, $"Pausing..."));
                        while (paused)
                        {
                        }
                        SimpleLogger(new(0, 0, i, temp.Count, $"Resuming..."));
                    }
                    results.Add(x);
                    cycles++;

                    if (cycles % Options.SimpleLoggingInterval is 0)
                        SimpleLogger(new(i, 0, cycles, temp.Count, $"thread {i} is finishing up and added {cycles}/{temp.Count} items to the collection."));
                });
                SimpleLogger(new(i, 0, parts[i].Count, parts[i].Count, $"thread {i} Finished."));
            });

            if (abort)
            {
                return new(new("Aborted"));
            }

            StringBuilder finalResult = new();
            results.OrderBy(result => result.Index).Foreach(result => finalResult.Append(result.Result));

            SimpleLogger("Finished serialization of collection");
            return new(finalResult);
        }

        private void AppendFieldsAndProperties<T>(T obj, StringBuilder builder)
        {
            try
            {
                ReflectionHelper<T> rh = ReflectionHelper<T>.ForObject(ref obj);
                var members = rh.GetMembers();

                foreach (var member in members)
                {
                    AppendData(rh, member, builder);
                }
            }
            catch (SerializationFailedException e)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new SerializationFailedException("Failed to serialize object.", e);
            }
        }

        private void AppendData<T>(ReflectionHelper<T> rh, MemberData member, StringBuilder builder)
        {
            try
            {
                if (member.HasAttribute<NotSerializedAttribute>())
                {
                    Logger($"Member {member.Name} is marked as non-serialized. Skipping...");
                    return;
                }
                if (!member.IsPublic)
                {
                    if (member.GetAttribute<SerializedAttribute>() is SerializedAttribute attribute)
                    {
                        foreach (var type in attribute.AllowedSerializers)
                        {
                            if (type != GetType())
                            {
                                Logger($"Member {member.Name} is not public and does not have the SerializableAttribute that allows the serializer of type {GetType().Name}. Skipping...");
                                return;
                            }
                        }
                    }
                    else
                    {
                        Logger($"Member {member.Name} is not public and does not have the SerializableAttribute. Skipping...");
                        return;
                    }
                }

                if (!member.CanWrite)
                {
                    Logger($"Member {member.Name} is not writable. Skipping");
                    return;
                }

                object? value = rh.GetValueFrom(member.Name);
                if (Options.IgnoreNullValues && value is null)
                    return;

                builder.Append(member.Name);
                builder.Append(Options.NameValueSeparator);
                builder.Append(value ?? Options.NullValue);
                builder.Append(Options.FieldSeparator);
            }
            catch (Exception e)
            {
                throw new SerializationFailedException($"Failed to serialize field or property {member.Name}", e);
            }
        }

        private void AppendType<T>(T obj, StringBuilder builder)
        {
            if (Options.IncludeTypeDefinition)
            {
                string typeNameAndAssembly = obj.GetType().AssemblyQualifiedName;
                builder.Append(typeNameAndAssembly);
                builder.Append(Options.TypeSeparator);
            }
        }

        /// <summary>
        /// Aborts the current serialization. Will unpause the serialization if it is paused to allow for the abort to be processed.
        /// </summary>
        public void Abort()
        {
            if (paused)
                Resume();
            abort = true;
        }

        /// <summary>
        /// If the serialization is paused, resumes it. <br></br>
        /// also allows for the serialization to be aborted again.
        /// </summary>
        public void Resume()
        {
            abort = false;
            paused = false;
        }

        /// <summary>
        /// Pauses the serialization. <br></br>
        /// </summary>
        public void Pause()
        {
            paused = true;
        }

        private record CollectionResult(StringBuilder Result, int Index);
    }
}
