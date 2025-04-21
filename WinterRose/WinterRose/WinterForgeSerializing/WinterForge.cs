using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

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
            using (Stream serialized = new MemoryStream())
            using (Stream opcodes = File.Open(path, FileMode.Create, FileAccess.ReadWrite))
            {
                ObjectSerializer serializer = new();
                serializer.Serialize(o, serialized);
                serialized.Seek(0, SeekOrigin.Begin);
                new HumanReadableParser().Parse(serialized, opcodes);
            }
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
            return Unsafe.As<T>(res);
        }
    }
}
