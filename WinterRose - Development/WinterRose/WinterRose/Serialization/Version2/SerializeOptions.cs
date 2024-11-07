using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.Serialization.Version2
{
    /// <summary>
    /// Options for the <see cref="SnowSerializer"/> class.
    /// </summary>
    public class SerializeOptions
    {
        /// <summary>
        /// The character that will be used to separate the name of a field or property from its value.
        /// </summary>
        public char NameValueSeparator { get; set; } = ':';
        /// <summary>
        /// The character that will be used to separate different fields and properties.
        /// </summary>
        public char FieldSeparator { get; set; } = ',';
        /// <summary>
        /// The character that will be used to separate the type of an object from its values.
        /// </summary>
        public char TypeSeparator { get; set; } = '|';
        /// <summary>
        /// Whether or not to include the type of an object in the serialized data.
        /// </summary>
        public bool IncludeTypeDefinition { get; set; } = true;
        /// <summary>
        /// The character that will be used to separate the values of an array.
        /// </summary>
        public char ArraySeparator { get; set; } = ',';
        /// <summary>
        /// The character that will be used to define the start of an array or list
        /// </summary>
        public char ArrayStart { get; set; } = '[';
        /// <summary>
        /// The character that will be used to define the end of an array or list
        /// </summary>
        public char ArrayEnd { get; set; } = ']';
        /// <summary>
        /// The character that will be used to define the start of an object
        /// </summary>
        public char ObjectStart { get; set; } = '{';
        /// <summary>
        /// The character that will be used to define the end of an object
        /// </summary>
        public char ObjectEnd { get; set; } = '}';
        /// <summary>
        /// The string that will be used to define a null value
        /// </summary>
        public string NullValue { get; set; } = "null";
        /// <summary>
        /// Whether or not to include null values in the serialized data.
        /// </summary>
        public bool IgnoreNullValues { get; set; } = true;
        /// <summary>
        /// The character that will be used to separate the key and value of a dictionary.
        /// </summary>
        public char DictionaryKeyValueSeparator { get; set; } = '=';
        /// <summary>
        /// The maximum number of threads that can be used to serialize or deserialze objects, defaults to the number of processors on the machine.
        /// </summary>
        public int MaxThreads { get; set; } = Environment.ProcessorCount;
        /// <summary>
        /// Every how many items handled should the <see cref="ISerializer"/> or <see cref="IDeserializer"/> log its progress.
        /// </summary>
        public int SimpleLoggingInterval { get; set; } = 1000;  
        public Dictionary<Type, Type> TypeSerializationSelection { get; set; } = new();

        /// <summary>
        /// Every how many items (<paramref name="interval"/>) handled should the <see cref="ISerializer"/> or <see cref="IDeserializer"/> log its progress.
        /// </summary>
        public SerializeOptions WithSimpleLoggingInterval(int interval)
        {
            SimpleLoggingInterval = interval;
            return this;
        }
        public SerializeOptions WithMaxThreads(int max)
        {
            MaxThreads = max;
            return this;
        }
        public SerializeOptions WithdictionaryKeyValueSeperator(char seperator)
        {
            DictionaryKeyValueSeparator = seperator;
            return this;
        }
        public SerializeOptions WithNameValueSeperator(char seperator)
        {
            NameValueSeparator = seperator;
            return this;
        }
        public SerializeOptions WithFieldSeperator(char seperator)
        {
            FieldSeparator = seperator;
            return this;
        }
        public SerializeOptions WithTypeSeperator(char seperator)
        {
            TypeSeparator = seperator;
            return this;
        }
        public SerializeOptions ShouldIncludeTypeDefinition(bool include)
        {
            IncludeTypeDefinition = include;
            return this;
        }
        public SerializeOptions WithArraySeperator(char seperator)
        {
            ArraySeparator = seperator;
            return this;
        }
        public SerializeOptions WithArrayStart(char start)
        {
            ArrayStart = start;
            return this;
        }
        public SerializeOptions WithArrayEnd(char end)
        {
            ArrayEnd = end;
            return this;
        }
        public SerializeOptions WithObjectStart(char start)
        {
            ObjectStart = start;
            return this;
        }
        public SerializeOptions WithObjectEnd(char end)
        {
            ObjectEnd = end;
            return this;
        }
        public SerializeOptions WithNullValue(string value)
        {
            NullValue = value;
            return this;
        }
        public SerializeOptions ShouldIgnoreNullValues(bool ignore)
        {
            IgnoreNullValues = ignore;
            return this;
        }
        public SerializeOptions WithTypeSerializationSelection<T, S>()
        {
            TypeSerializationSelection.Add(typeof(T), typeof(S));
            return this;
        }
        public SerializeOptions WithTypeSerializationSelection(Type t, Type s)
        {
            TypeSerializationSelection.Add(t, s);
            return this;
        }
    }
}
