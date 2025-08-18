using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.ForgeMantle.Models;
using WinterRose.ForgeMantle.Serialization;
using WinterRose.WinterForgeSerializing;

namespace WinterRose.ForgeMantle;
public class WindowsRegistryStorage : IConfigStorage
{
    public string ConfigName { get; }
    public IConfigSerializer Serializer => null;
    public IStringSerializer<ConfigSnapshot> StringSerializer { get; }
    public WindowsRegistryStorage(string configName, IStringSerializer<ConfigSnapshot> serializer)
    {
        ConfigName = configName;
        StringSerializer = serializer;
    }

    public ConfigSnapshot? Load()
    {
        return RegPrefs.GetSerialized<ConfigSnapshot>(StringSerializer, ConfigName);
    }
    public void Save(ConfigSnapshot snapshot)
    {
        RegPrefs.SetSerialized(StringSerializer, ConfigName, snapshot);
    }

    /// <summary>
    /// A class that provides simple access to store and retrieve values in the application specific registry folder for the current windows user.
    /// </summary>
    public class RegPrefs
    {
        static RegKey rootKey;

        static RegPrefs()
        {
            string appName = System.Reflection.Assembly.GetEntryAssembly()?.GetName().Name ?? throw new InvalidOperationException("RegPrefs can only be used when an entry assembly is defined.");
            rootKey = RegKey.CurrentUser(true, true)[appName];
        }

        /// <summary>
        /// Sets a string value
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void SetString(string key, string value)
        {
            rootKey[key].SetValue(value);
        }

        /// <summary>
        /// Gets a string value
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static string GetString(string key, string defaultValue = null)
        {
            if (!rootKey[key].GetValue(out string s))
                return defaultValue;
            return s;
        }

        /// <summary>
        /// Sets a value that is serialized using <see cref="WinterForge"/>
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void SetSerialized<T>(IStringSerializer<T> serializer, string key, T value)
        {
            string serialize = serializer.Serialize(value);
            rootKey[key].SetValue(serialize);
        }

        /// <summary>
        /// Gets a value that is serialized using <see cref="WinterForge"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static T GetSerialized<T>(IStringSerializer<T> serializer, string key, T defaultValue = default)
        {
            if (!rootKey[key].GetValue(out string s))
                return defaultValue;
            return (T)serializer.Deserialize(s);
        }

        /// <summary>
        /// Sets an integer value
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void SetInt(string key, int value)
        {
            rootKey[key].SetValue(value);
        }

        /// <summary>
        /// Gets an integer value
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static int GetInt(string key, int defaultValue = 0)
        {
            if (!rootKey[key].GetValue(out int i))
                return defaultValue;
            return i;
        }

        /// <summary>
        /// Sets a boolean value
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void SetBool(string key, bool value)
        {
            rootKey[key].SetValue(value);
        }

        /// <summary>
        /// Gets a boolean value
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static bool GetBool(string key, bool defaultValue = false)
        {
            if (!rootKey[key].GetValue(out bool s))
                return defaultValue;
            return s;
        }


        public static void SetFloat(string key, float value)
        {
            rootKey[key].SetValue(value.ToString());
        }

        public static float GetFloat(string key, float defaultValue = 0)
        {
            if (!rootKey[key].GetValue(out string s))
                return defaultValue;
            return float.Parse(s);
        }

        /// <summary>
        /// Deletes all values.
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        public static void Flush()
        {
            rootKey.Delete();
        }
    }

    /// <summary>
    /// An easier abstraction for working with the Windows Registry. (Otherwise know as <see cref="RegistryKey"/>)
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay}")]
    public class RegKey : IDisposable
    {
        RegistryKey? key;
        RegKey? parent;
        string name;

        bool writable;
        bool autoCreateOnAbsentKey;

        public bool IsDisposed { get; private set; }
        public int DefaultValueAsInt
        {
            get
            {
                if (key is null)
                    throw new InvalidOperationException("Cannot read from a null registry key.");

                if (key.GetValue("") is int val)
                    return val;
                else
                    throw new InvalidOperationException("The default value is not an integer.");
            }
        }
        public string DefaultValueAsString
        {
            get
            {
                if (key is null)
                    throw new InvalidOperationException("Cannot read from a null registry key.");

                if (key.GetValue("") is string val)
                    return val;
                else
                    throw new InvalidOperationException("The default value is not a string.");
            }
        }
        public bool Exists => key != null;

        public string[] DefaultValueAsStringArray
        {
            get
            {
                if (key is null)
                    throw new InvalidOperationException("Cannot read from a null registry key.");

                if (key.GetValue("") is string[] val)
                    return val;
                else
                    throw new InvalidOperationException("The default value is not a string array.");
            }
        }

        private RegKey(RegKey? keyParent, RegistryKey regParent, string key, bool writable, bool autoCreateOnAbsentKey)
        {
            name = key;
            if (name == "")
                name = regParent.Name;

            if (key == "")
                this.key = regParent;
            else
            {
                this.key = regParent.OpenSubKey(key, writable);
                this.parent = keyParent;
            }

            if (autoCreateOnAbsentKey)
                this.key ??= regParent.CreateSubKey(key, writable);

            this.writable = writable;
            this.autoCreateOnAbsentKey = autoCreateOnAbsentKey;
        }

        private string DebuggerDisplay
        {
            get
            {
                if (IsDisposed)
                    return $"Disposed Key '{name}'";

                return key?.Name ?? "Non existing key: " + name;
            }
        }

        /// <summary>
        /// Opens a registry key for the CLASSES_ROOT hive.
        /// </summary>
        /// <param name="writable"></param>
        /// <param name="autoCreateOnAbsentKey"></param>
        /// <returns></returns>
        public static RegKey ClassesRoot(bool writable = false, bool autoCreateOnAbsentKey = false) => new RegKey(null, Registry.ClassesRoot, "", writable, autoCreateOnAbsentKey);
        /// <summary>
        /// Opens a registry key for the CURRENT_CONFIG hive.
        /// </summary>
        /// <param name="writable"></param>
        /// <param name="autoCreateOnAbsentKey"></param>
        /// <returns></returns>
        public static RegKey CurrentConfig(bool writable = false, bool autoCreateOnAbsentKey = false) => new RegKey(null, Registry.CurrentConfig, "", writable, autoCreateOnAbsentKey);
        /// <summary>
        /// Opens a registry key for the CURRENT_USER hive.
        /// </summary>
        /// <param name="writable"></param>
        /// <param name="autoCreateOnAbsentKey"></param>
        /// <returns></returns>
        public static RegKey CurrentUser(bool writable = false, bool autoCreateOnAbsentKey = false) => new RegKey(null, Registry.CurrentUser, "", writable, autoCreateOnAbsentKey);
        /// <summary>
        /// Opens a registry key for the LOCAL_MACHINE hive.
        /// </summary>
        /// <param name="writable"></param>
        /// <param name="autoCreateOnAbsentKey"></param>
        /// <returns></returns>
        public static RegKey LocalMachine(bool writable = false, bool autoCreateOnAbsentKey = false) => new RegKey(null, Registry.LocalMachine, "", writable, autoCreateOnAbsentKey);
        /// <summary>
        /// Opens a registry key for the USERS hive.
        /// </summary>
        /// <param name="writable"></param>
        /// <param name="autoCreateOnAbsentKey"></param>
        /// <returns></returns>
        public static RegKey Users(bool writable = false, bool autoCreateOnAbsentKey = false) => new RegKey(null, Registry.Users, "", writable, autoCreateOnAbsentKey);


        public bool HasValue(string valueName) => key is not null && key.GetValue(valueName) != null;
        public bool IsWritable() => writable || key is null;

        public bool SetValue<T>(string valueName, T value)
        {
            if (key is null)
                throw new InvalidOperationException("Cannot write to a null registry key.");
            if (!writable)
                throw new InvalidOperationException("Cannot write to a read-only registry key.");

            RegistryValueKind valueKind = GetRegistryValueKind(value);
            key.SetValue(valueName, value, valueKind);

            var val = key.GetValue(valueName);
            return val.Equals(value);
        }

        public bool SetValue(string[] strings) => SetValue<string[]>("", strings);
        public bool SetValue(string valueName, string[] strings) => SetValue<string[]>(valueName, strings);

        public bool SetValue<T>(T value)
        {
            if (key is null)
                throw new InvalidOperationException("Cannot write to a null registry key.");
            if (!writable)
                throw new InvalidOperationException("Cannot write to a read-only registry key.");

            RegistryValueKind valueKind = GetRegistryValueKind(value);
            key.SetValue("", value, valueKind);

            object? val = key.GetValue("");
            return val is not null && val.Equals(value);
        }

        public bool GetValue<T>(string valueName, out T value)
        {
            if (key is null)
                throw new InvalidOperationException("Cannot write to a null registry key.");

            var val = key.GetValue(valueName);
            if (val is T v)
            {
                value = v;
                return true;
            }
            else if (val is not null)
                throw new InvalidOperationException("The value is not of the correct type.");
            else
            {
                value = default;
                return false;
            }
        }

        public bool GetValue(string keyName, out string value) => GetValue<string>(keyName, out value);

        public bool GetValue<T>(out T value)
        {
            if (key is null)
                throw new InvalidOperationException("Cannot write to a null registry key.");

            var val = key.GetValue("");
            if (val is T v)
            {
                value = v;
                return true;
            }
            else if (val is not null)
                throw new InvalidOperationException("The value is not of the correct type.");
            else
            {
                value = default;
                return false;
            }
        }

        public bool GetValue(out string value)
        {
            GetValue<object>(out var v);
            value = v?.ToString() ?? "";
            return value != "";
        }

        /// <summary>
        /// Gets the type of the default value of the registry key.
        /// </summary>
        /// <returns>Returns the type if the key has a value, other wise <see langword="null"/></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public Type? GetValueType()
        {
            if (key is null)
                throw new InvalidOperationException("Cannot read from a null registry key.");

            var val = key.GetValue("");
            return val?.GetType() ?? null;
        }

        /// <summary>
        /// Gets the type of the value of the registry key.
        /// </summary>
        /// <param name="valueName"></param>
        /// <returns>Returns the type if the key has a value, other wise <see langword="null"/></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public Type? GetValueType(string valueName)
        {
            if (key is null)
                throw new InvalidOperationException("Cannot read from a null registry key.");

            var val = key.GetValue(valueName);
            return val?.GetType() ?? null;
        }

        public bool DeleteValue(string valueName)
        {
            if (key is null)
                throw new InvalidOperationException("Cannot write to a null registry key.");
            if (!writable)
                throw new InvalidOperationException("Cannot write to a read-only registry key.");

            key.DeleteValue(valueName);
            return key.GetValue(valueName) == null;
        }

        public void Close() => key.Close();

        public RegKey this[string key]
        {
            get
            {
                //if(!writable)
                //throw new InvalidOperationException("Cannot write to a read-only registry key.");

                var newKey = new RegKey(this, this.key, key, writable, autoCreateOnAbsentKey);
                return newKey;
            }
        }

        /// <summary>
        /// Closes the registry key and releases all resources.
        /// </summary>
        public void Dispose() => Dispose(false);

        /// <summary>
        /// Disposes the registry and optionally disposes the parent key recursively.
        /// </summary>
        /// <param name="includeParentKeys"></param>
        public void Dispose(bool includeParentKeys)
        {
            if (IsDisposed) return;

            if (includeParentKeys)
                parent?.Dispose(includeParentKeys);

            if (key is not null)
            {
                key.Close();
                key.Dispose();
                key = null;
            }

            IsDisposed = true;
        }

        private RegistryValueKind GetRegistryValueKind(object value)
        {
            return value switch
            {
                int _ => RegistryValueKind.DWord,
                long _ => RegistryValueKind.QWord,
                string _ => RegistryValueKind.String,
                string[] _ => RegistryValueKind.MultiString,
                byte[] _ => RegistryValueKind.Binary,
                _ => throw new ArgumentException("Unsupported registry value type.")
            };
        }

        /// <summary>
        /// Deletes the registry key and all its subkeys, then disposes the object.
        /// </summary>
        public void Delete()
        {
            foreach (var subKey in key.GetSubKeyNames())
            {
                key.DeleteSubKeyTree(subKey, false);
            }

            // get parent key

            Dispose();
        }
    }
}
