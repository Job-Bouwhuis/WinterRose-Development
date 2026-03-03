using System;
using System.Collections.Generic;
using WinterRose.Recordium;

namespace WinterRose.ArgumentUtility;

public class ProgramArguments
{
    private readonly Dictionary<string, string> parameters = new(StringComparer.OrdinalIgnoreCase);

    private static Log log = new("ProgramArguments");

    public ProgramArguments(string[] args)
    {
        Parse(args);
    }

    public static implicit operator ProgramArguments(string[] args) => new(args);

    private void Parse(string[] args)
    {
        string pendingKey = null;

        foreach (var arg in args)
        {
            if (arg.StartsWith("--"))
            {
                // Flush previous pending key if no value was assigned
                if (pendingKey != null)
                {
                    parameters[pendingKey] = "true";
                    pendingKey = null;
                }

                string cleanArg = arg[2..]; // remove leading --

                int equalsIndex = cleanArg.IndexOf('=');

                if (equalsIndex >= 0)
                {
                    string key = cleanArg[..equalsIndex].Trim();
                    string value = cleanArg[(equalsIndex + 1)..].Trim();
                    parameters[key] = value;
                }
                else
                {
                    // Might still get a value from next arg
                    pendingKey = cleanArg;
                }
            }
            else
            {
                if (pendingKey != null)
                {
                    // Append this arg to pending value, space-separated if already exists
                    if (parameters.ContainsKey(pendingKey))
                        parameters[pendingKey] += " " + arg;
                    else
                        parameters[pendingKey] = arg;

                    pendingKey = null;
                }
                else
                {
                    WarnUnknown(arg);
                }
            }
        }

        // If the last key was pending with no value, set it to true
        if (pendingKey != null)
            parameters[pendingKey] = "true";
    }

    private void WarnUnknown(string arg) => 
        log.Warning($"Unrecognized argument format: '{arg}'. Arguments should start with '--' " +
            $"and optionally contain '=' for values. " +
            $"Example: --key=value or --flag");

    public string Get(string key, string defaultValue = null)
        => parameters.TryGetValue(key, out var val) ? val : defaultValue;

    public int GetInt(string key, int defaultValue = 0)
        => int.TryParse(Get(key), out var val) ? val : defaultValue;

    public bool GetBool(string key, bool defaultValue = false)
        => bool.TryParse(Get(key), out var val) ? val : defaultValue;

    public float GetFloat(string key, float defaultValue = 0f)
        => float.TryParse(Get(key), out var val) ? val : defaultValue;
}