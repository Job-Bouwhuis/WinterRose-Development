using Microsoft.CodeAnalysis.Operations;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WinterRose.Exceptions;
using WinterRose.FileManagement;
using WinterRose.Reflection;

namespace WinterRose;

public sealed class SolarPanelReader
{
    public static SolarPanelTypeSearchOverrideCollection TypeOverrides { get; } = new();

    public static SolarPanel Read(string path)
    {
        SolarPanel result = new();

        foreach (var (line, i) in File.ReadAllLines(path).Select((x, i) => (x, i)))
        {
            if (i is 0)
            {
                if (line is "[SOLARPANEL]")
                    continue;
                else
                    throw new SolarpanelLoadException("Invalid File.");
            }
            if (string.IsNullOrWhiteSpace(line))
                continue;

            string[] parts = line.Split('=', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            if (parts.Length == 1)
                throw new SolarpanelLoadException($"Invalid property line: {line}");

            dynamic value = GetAssigningValue(parts[1], result);
            _ = SetVar(result, parts[0].Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries), value);
        }
        return result;
    }
    public static SolarPanel Read(string path, params SolarPanelLoadTypeOverride[] typeOverrides)
    {
        TypeOverrides.AddRange(typeOverrides);
        return Read(path, typeOverrides);
    }

    private static dynamic? SetVar(SolarPanel obj, string[] vars, dynamic value)
    {
        dynamic? cur = null;

        Type? t = SelectType(TypeWorker.FindType(vars[0]), vars[0]);
            for (int i = 0; i < vars.Length - 1; i++)
            {
                string? vari = vars[i];
            ReflectionHelper refh = ReflectionHelper.ForObject(cur is null ? obj : cur);
                cur = refh.GetValueFrom(vari);
            }

        ReflectionHelper rh = ReflectionHelper.ForObject(cur ?? obj);
        rh.SetValue(vars.Last(), value);
        return cur;
    }
    private static dynamic GetAssigningValue(string content, SolarPanel obj)
    {
        if (IsPrimitive(content, out dynamic primitive))
        {
            return primitive;
        }

        int typeDefEnd = content.IndexOf("(");
        if (typeDefEnd == -1) typeDefEnd = content.Length;
        string typeDef = content[..typeDefEnd].Trim();

        if (typeDef.Contains('.'))
        {
            return GetVariable(content, obj);
        }

        Type type = SelectType(TypeWorker.FindType(typeDef), typeDef);

        if (type == null)
        {
            throw new TypeNotFoundException($"Couldnt find the type of name \"{typeDef}\"");
        }

        string rest = content[typeDefEnd..].TrimStart('(').TrimEnd(')');
        string[] args = rest.Split(',', StringSplitOptions.TrimEntries);

        dynamic instance = GetInstanceOf(type, args);

        return instance;
    }
    private static bool IsPrimitive(string data, out dynamic primitive)
    {
        if (data.Contains('(') || data.Contains(')'))
        {
            primitive = null;
            return false;
        }
        if (data.Contains(".."))
        {
            int dotdotIndex = data.IndexOf("..");
            string[] nums = new string[2] { data[..dotdotIndex], data[(dotdotIndex + 1)..] };
            foreach (int i in nums.Length)
                nums[i] = nums[i].Trim().TrimStart('.').TrimEnd('.');
            if (nums.Length != 2)
            {
                primitive = null;
                return false;
            }
            try
            {
                int start = string.IsNullOrWhiteSpace(nums[0]) ? 0 : TypeWorker.CastPrimitive<int>(nums[0]);
                int end = string.IsNullOrWhiteSpace(nums[1]) ? 0 : TypeWorker.CastPrimitive<int>(nums[1]);
                primitive = new Range(new(start), new(end));
                return true;
            }
            catch (FailedToCastTypeException)
            {
                primitive = null;
                return false;
            }
        }

        data = data.Replace('.', ',');
        primitive = null;
        if (data.StartsWith('"') && data.EndsWith('"'))
        {
            primitive = data.TrimStart('"').TrimEnd('"');
            return true;
        }
        if (data.ToLower() is "true" or "false")
        {
            primitive = TypeWorker.CastPrimitive<bool>(data);
            return true;
        }

        foreach (char c in data)
        {
            if (!(c.IsNumber() || c is ',' or '_' or 'f' or 'd' or 'L' or '-'))
                return false;
        }
        int index = data.IndexOf('f');
        if (index is not -1)
            if (index != data.Length - 1)
                return false;
            else
            {
                primitive = TypeWorker.CastPrimitive<float>(data.TrimEnd('f'));
                return true;
            }

        index = data.IndexOf('d');
        if (index is not -1)
            if (index != data.Length - 1)
                return false;
            else
            {
                primitive = TypeWorker.CastPrimitive<double>(data.TrimEnd('d'));
                return true;
            }
        index = data.IndexOf('L');
        if (index is not -1)
            if (index != data.Length - 1)
                return false;
            else
            {
                primitive = TypeWorker.CastPrimitive<long>(data.TrimEnd('L'));
                return true;
            }
        primitive = TypeWorker.CastPrimitive<int>(data);
        return true;
    }
    private static dynamic GetVariable(string content, SolarPanel obj)
    {
        string[] parts = content.Split('.', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

        dynamic cur = null;

        try
        {
            for (int i = 0; i < parts.Length; i++)
            {
                string? part = parts[i];
                ReflectionHelper refh = ReflectionHelper.ForObject(cur is null ? obj : cur);
                cur = refh.GetValueFrom(part);
            }
        }
        catch (FieldNotFoundException)
        {
            cur = GetValue(content);
        }

        return cur;
    }
    private static dynamic GetInstanceOf(Type type, params string[] args)
    {
        foreach (var c in type.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
        {
            var parameters = c.GetParameters();
            object?[] castedArgs = ValidateArgs(args, parameters);
            if (parameters.Length != castedArgs.Length || castedArgs.Length != args.Length)
                goto Continue;
            foreach (var (arg, i) in parameters.Select((arg, i) => (arg, i)))
            {
                bool b = castedArgs[i].GetType().IsAssignableTo(arg.ParameterType);
                bool b2 = arg.ParameterType != castedArgs[i].GetType();
                if (!b && b2)
                    goto Continue;
            }

            return c.Invoke(castedArgs);

        Continue:
            continue;
        }
        return null;
    }
    private static dynamic GetValue(string arg)
    {
        string[] vars = arg.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        dynamic? cur = null;
        for (int i = 0; i < vars.Length - 1; i++)
        {
            string? vari = vars[i];

            Type curType = SelectType(TypeWorker.FindType(vari), vari);

            ReflectionHelper refh;
            if (cur is null)
            {
                refh = ReflectionHelper.ForType(curType);
            }
            else
            {
                if (cur.GetType() == typeof(Type))
                    refh = ReflectionHelper.ForType(cur);
                else
                    refh = ReflectionHelper.ForObject(cur);
            }

            cur = refh.GetValueFrom(vars[i + 1]);
        }

        return cur;
    }
    private static object?[] ValidateArgs(string[] args, ParameterInfo[] parameters)
    {
        object?[] castedArgs = new object?[args.Length];

        if (parameters.Length != args.Length)
        {
            int optionalparamsCount = parameters.Where(p => p.IsOptional).Count();
            if (parameters.Length - optionalparamsCount < args.Length)
                return Array.Empty<object?>();
            else
            {
                foreach (var arg in parameters.Where(p => p.IsOptional))
                {
                    int nonOptional = ~(parameters.Where(p => p.IsOptional).Count() - parameters.Length) + 1;
                    if (arg.IsOptional)
                    {
                        var par = parameters[nonOptional];
                        if (arg.ParameterType == par.ParameterType)
                            continue;
                        args = args.Append(arg.DefaultValue?.ToString()).ToArray()!;
                        castedArgs = new object?[args.Length];
                    }
                }
            }
        }
        if (castedArgs.All(x => x is null))
            for (int i = 0; i < args.Length; i++)
            {
                string? arg = args[i];

                Type t = SelectType(TypeWorker.FindType(arg), arg);

                if (IsPrimitive(arg, out var res))
                {
                    castedArgs[i] = res;
                }
                else if (arg.Contains('.'))
                {
                    var value = GetValue(arg);
                    castedArgs[i] = value;
                }
                else
                    castedArgs[i] = Convert.ChangeType(arg, parameters[i].ParameterType);

            }

        return castedArgs;
    }
    private static Type? SelectType(Type type, string typeName = "")
    {
        foreach (var t in TypeOverrides)
        {
            if (type is not null)
            {
                if (type.Name == t.Identifier)
                {
                    return t.Type;
                }
            }
            else
                if (typeName == t.Identifier)
                return t.Type;
        }

        type ??= TypeWorker.FindType(typeName);
        return type;
    }
}

[DebuggerDisplay("{Identifier} = {Type.FullName}")]
public class SolarPanelLoadTypeOverride
{
    public Type Type { get; private set; }
    internal string Identifier { get; private set; }

    internal SolarPanelLoadTypeOverride(Type type, string identifyer)
    {
        Type = type;
        Identifier = identifyer;
    }
    internal static List<SolarPanelLoadTypeOverride> GetDefinitions(string[] data)
    {
        List<SolarPanelLoadTypeOverride> defs = new List<SolarPanelLoadTypeOverride>();

        foreach (string over in data)
        {
            string[] def = over.Split("=", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            Type t = TypeWorker.FindType(def[1]);
            defs.Add(new(t, def[0]));
        }
        return defs;
    }
    internal static async Task<List<SolarPanelLoadTypeOverride>> GetDefinitionsAsync(string[] data)
    {
        List<SolarPanelLoadTypeOverride> defs = new List<SolarPanelLoadTypeOverride>();

        List<Task<List<SolarPanelLoadTypeOverride>>> tasks = new();

        foreach (string over in data)
        {
            string[] def = over.Split("=", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            Type t = TypeWorker.FindType(def[1]);
            defs.Add(new(t, def[0]));
        }
        return defs;
    }
}

[Serializable]
public class SolarpanelLoadException : Exception
{
    public SolarpanelLoadException() { }
    public SolarpanelLoadException(string message) : base(message) { }
    public SolarpanelLoadException(string message, Exception inner) : base(message, inner) { }
    protected SolarpanelLoadException(
      System.Runtime.Serialization.SerializationInfo info,
      System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}