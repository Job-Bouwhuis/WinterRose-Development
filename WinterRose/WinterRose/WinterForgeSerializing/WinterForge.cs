using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
            using MemoryStream opcodes = new();

            ObjectSerializer serializer = new();
            DoSerialization(serializer, o, serialized, opcodes);
            opcodes.Seek(0, SeekOrigin.Begin);
            byte[] bytes = opcodes.ToArray();
            return Encoding.Unicode.GetString(bytes);
        }

        private static void DoSerialization(ObjectSerializer serializer, object o, Stream serialized, Stream opcodes)
        {
            SerializeAsAttributeINTERNAL? attr = o.GetType().GetCustomAttribute<SerializeAsAttributeINTERNAL>();
            Type t = attr?.GetType() ?? o.GetType();

            if (t.IsAssignableTo(typeof(IEnumerable)))
            {
                serializer.ClearOnRoot = false;
                IEnumerable e = (IEnumerable)o;
                foreach (var item in e)
                    serializer.Serialize(item, serialized, true, false);
                serializer.ClearOnRoot = true;
            }
            else
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
        public static T DeserializeFromFile<T>(FilePath path) where T : class
        {
            using Stream opcodes = File.OpenRead(path);
            var instructions = InstructionParser.ParseOpcodes(opcodes);
            object res = new InstructionExecutor().Execute(instructions);
            Type r = res.GetType();

            if (res is List<object> e && (typeof(T).IsArray || typeof(T).Name.Contains("List`1")))
            {
                Type t = typeof(T);
                if (t.Name.Contains("List`1"))
                    t = t.GetGenericArguments()[0];
                else
                    t = t.GetElementType()!;

                IEnumerable<object> a = e.Where(x => x.GetType() == t);
                if (typeof(T).IsArray)
                    res = a.ToArray();
                else if (typeof(T).Name.Contains("List`1"))
                    res = a.ToList();
                else
                    res = a;
            }

            return Unsafe.As<T>(res);
        }
    }
}
