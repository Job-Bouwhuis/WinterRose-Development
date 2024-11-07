using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.Serialization;

namespace WinterRose;

/// <summary>
/// A class that provides simple access to store and retrieve values in the application specific registry folder for the current windows user.
/// </summary>
public static class RegPrefs
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
        if(!rootKey[key].GetValue(out string s))
            return defaultValue;
        return s;
    }

    /// <summary>
    /// Sets a value that is serialized using <see cref="SnowSerializer"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public static void SetSerialized<T>(string key, T value)
    {
        string serialize = SnowSerializer.Serialize(value).Result;

        rootKey[key].SetValue(serialize);
    }

    /// <summary>
    /// Gets a value that is serialized using <see cref="SnowSerializer"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    public static T GetSerialized<T>(string key, T defaultValue = default)
    {
        if (!rootKey[key].GetValue(out string s))
            return defaultValue;
        return SnowSerializer.Deserialize<T>(s).Result;
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
        string s = value ? "BOOLEAN_TRUE" : "BOOLEAN_FALSE";
        rootKey[key].SetValue(s);
    }

    /// <summary>
    /// Gets a boolean value
    /// </summary>
    /// <param name="key"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    public static bool GetBool(string key, bool defaultValue = false)
    {
        if (!rootKey[key].GetValue(out string s))
            return defaultValue;
        return s == "BOOLEAN_TRUE";
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
