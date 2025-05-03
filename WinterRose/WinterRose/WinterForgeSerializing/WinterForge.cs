using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using WinterRose.FileManagement;
using WinterRose.Serialization;
using WinterRose.WinterForgeSerializing.Formatting;
using WinterRose.WinterForgeSerializing.Workers;

namespace WinterRose.WinterForgeSerializing
{
    /// <summary>
    /// The main delegation class for the WinterForge serialization system
    /// </summary>
    public static class WinterForge
    {
        /// <summary>
        /// Serializes the given object directly to opcodes for fastest deserialization
        /// </summary>
        /// <param name="o"></param>
        /// <param name="path"></param>
        public static void SerializeToFile(object o, FilePath path)
        {
            List<string> paths = path.ToString().Split(['/', '\\']).ToList();
            if (paths.Count > 1)
            {
                paths.RemoveAt(paths.Count - 1);

                string directory = string.Join("/", paths);
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

            }

            using (Stream serialized = new MemoryStream())
            using (Stream opcodes = File.Open(path, FileMode.Create, FileAccess.ReadWrite))
            //using (Stream formatted = File.OpenWrite("lasthumanreadable.txt"))
            {
                ObjectSerializer serializer = new();
                DoSerialization(serializer, o, serialized, opcodes);
            }
        }

        public static string SerializeToString(object o)
        {
            using MemoryStream serialized = new();
            using MemoryStream formatted = new();

            ObjectSerializer serializer = new();
            serializer.Serialize(o, serialized);
            serialized.Seek(0, SeekOrigin.Begin);

            new HumanReadableIndenter().Process(serialized, formatted);

            byte[] bytes = formatted.ToArray();
            return Encoding.UTF8.GetString(bytes);
        }

        private static void DoSerialization(ObjectSerializer serializer, object o, Stream serialized, Stream opcodes)
        {
            serializer.Serialize(o, serialized);
            serialized.Seek(0, SeekOrigin.Begin);

            //new HumanReadableIndenter().Process(serialized, formatted);
            //serialized.Seek(0, SeekOrigin.Begin);

            new HumanReadableParser().Parse(serialized, opcodes);
        }

        /// <summary>
        /// Deserializes the given file and returns the casted object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <returns></returns>
        public static T DeserializeFromFile<T>(FilePath path, Action<ProgressMark>? progress = null) where T : class
        {
            using Stream opcodes = File.OpenRead(path);
            var instructions = InstructionParser.ParseOpcodes(opcodes);
            return (T)DoDeserialization(typeof(T), instructions, progress);
        }

        private static object DoDeserialization(Type targetType, List<Instruction> instructions, Action<ProgressMark>? progress)
        {
            using var executor = new InstructionExecutor();
            if (progress is not null)
                executor.ProgressMark += progress;
            object res = executor.Execute(instructions);

            if (res is List<object> list && (targetType.IsArray || targetType.Name.Contains("List`1")))
            {
                if (targetType.IsArray)
                {
                    var array = Array.CreateInstance(targetType.GetElementType()!, list.Count);

                    for (int i = 0; i < list.Count; i++)
                        array.SetValue(list[i], i);

                    return array;
                }

                if (targetType.Name.Contains("List`1"))
                {
                    var targetList = WinterUtils.CreateList(targetType.GetGenericArguments()[0]);

                    for (int i = 0; i < list.Count; i++)
                        targetList.Add(list[i]);

                    return targetList;
                }

                throw new Exception("invalid deserialization!");
            }

            return res;
        }

        public static T DeserializeFromHumanReadableString<T>(string HumanReadable, Action<ProgressMark>? progress = null)
        {
            using var opcodes = new MemoryStream();
            using var serialized = new MemoryStream();
            byte[] humanBytes = Encoding.UTF8.GetBytes(HumanReadable);
            serialized.Write(humanBytes, 0, humanBytes.Length);
            serialized.Seek(0, SeekOrigin.Begin);
            new HumanReadableParser().Parse(serialized, opcodes);

            opcodes.Seek(0, SeekOrigin.Begin);

            var instructions = InstructionParser.ParseOpcodes(opcodes);

            return (T)DoDeserialization(typeof(T), instructions, progress);
        }

        public static void SerializeToStream(object obj, Stream data)
        {
            using MemoryStream serialized = new MemoryStream();
            ObjectSerializer serializer = new();
            DoSerialization(serializer, obj, serialized, data);
        }

        /// <summary>
        /// Deserializes the data from the given stream
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stream"></param>
        /// <param name="progress"></param>
        /// <returns></returns>
        public static T DeserializeFromStream<T>(Stream stream, Action<ProgressMark>? progress = null)
        {
            var instr = InstructionParser.ParseOpcodes(stream);
            return (T)DoDeserialization(typeof(T), instr, progress);
        }
    }
}
