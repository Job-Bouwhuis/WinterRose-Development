using SharpDX.WIC;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Runtime.InteropServices.ObjectiveC;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WinterRose.Exceptions;
using WinterRose.FileManagement;
using WinterRose.Monogame.Worlds.WorldSaving;
using WinterRose.Reflection;
using WinterRose.Serialization;

namespace WinterRose.Monogame.Worlds;

internal sealed class WorldTemplateLoader
{
    public const string COMMENT = "//";
    World world;
    public event Action<string> Callback = delegate { };
    List<WorldTemplateVariable> variables = new();

    internal WorldTemplateLoader(World world) => this.world = world;
    internal void LoadTemplate(string path)
    {
        var sw = Stopwatch.StartNew();

        Regex regex = new(@"object\s+\w+:[\s\S]*?end\s+\w+");

        string fileContent = FileManager.Read(path);
        fileContent = fileContent.Replace("\t", "");
        if (fileContent.Length is 0)
            return;

        int firstObjIndex = fileContent.IndexOf("object");
        string beforeFirstObject = fileContent[..firstObjIndex];
        List<string> typeOverrideDefinitions = new();
        string[] lines = fileContent[..firstObjIndex].Split("\n", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        lines = lines.Select(x => x.Trim().Replace('\r', '\0')).ToArray();

        foreach (var line in lines)
        {
            if (line.StartsWith("var"))
            {
                string[] parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                int equalsIndex = parts.ToList().IndexOf("=");
                string val = string.Join(' ', parts[(equalsIndex + 1)..]);
                variables.Add(new(parts[1], val));
            }
            else
                typeOverrideDefinitions.Add(line);
        }

        Callback("Loading type search overrides...");
        var specificDefinitions = WorldTemplateTypeSearchOverride.GetDefinitions(typeOverrideDefinitions.ToArray());

        bool error = specificDefinitions.Any(x => x.Type is null);
        foreach (var item in specificDefinitions.Where(x => x.Type is null))
        {
            var e = new TypeNotFoundException($"Couldnt find the type accosiated with identifyer \"{item.Identifier}\"");
            e.Source = "WorldTemplateLoader - Loading type definitions";
            Debug.LogException(e);
        }
        if (error)
            return;
        Callback("loading objects...");

        var matches = regex.Matches(fileContent);

        foreach ((Match match, int i) in matches.Cast<Match>().Select((x, i) => (x, i)))
        {
            if (fileContent[match.Index - 1] == '/')
                continue;
            if (!ParseTemplateObject(match.Value, specificDefinitions, null))
            {
                return;
            }
            Callback(MathS.GetPercentage(i, matches.Count, 2).ToString());
        }

        sw.Stop();

        Console.WriteLine($"loading complete in {sw.Elapsed.TotalMilliseconds}ms");
    }
    internal void LoadTemplateultiThread(string path)
    {
        var sw = Stopwatch.StartNew();

        Regex regex = new(@"object\s+\w+:[\s\S]*?end\s+\w+");
        string fileContent = FileManager.Read(path);
        fileContent = fileContent.Replace("\t", "");
        if (fileContent.Length is 0)
            return;

        int firstObjIndex = fileContent.IndexOf("object");
        string beforeFirstObject = fileContent[..firstObjIndex];
        List<string> typeOverrideDefinitions = new();
        string[] lines = fileContent[..firstObjIndex].Split("\n", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        lines = lines.Select(x => x.Trim().Replace('\r', '\0')).ToArray();

        foreach (var line in lines)
        {
            if (line.StartsWith("var"))
            {
                string[] parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                int equalsIndex = parts.ToList().IndexOf("=");
                string val = string.Join(' ', parts[(equalsIndex + 1)..]);
                variables.Add(new(parts[1], val));
            }
            else
                typeOverrideDefinitions.Add(line);
        }

        Callback("Loading type search overrides...");
        var specificDefinitions = WorldTemplateTypeSearchOverride.GetDefinitions(typeOverrideDefinitions.ToArray());

        bool error = specificDefinitions.Any(x => x.Type is null);
        foreach (var item in specificDefinitions.Where(x => x.Type is null))
        {
            var e = new TypeNotFoundException($"Couldnt find the type accosiated with identifyer \"{item.Identifier}\"");
            e.Source = "WorldTemplateLoader - Loading type definitions";
            Debug.LogException(e);
        }
        if (error)
            return;
        Callback("loading objects...");

        var matches = regex.Matches(fileContent);

        List<Task<bool>> tasks = new();
        ConcurrentStack<WorldObject> objectStack = new();

        foreach (Match match in matches.Cast<Match>())
        {
            if (fileContent[match.Index - 1] == '/')
                continue;
            tasks.Add(Task.Run(() => ParseTemplateObject(match.Value, specificDefinitions, objectStack)));
        }

        while (true)
        {
            bool objectLeft = objectStack.TryPeek(out _);
            bool tasksCompleted = tasks.All(x => x.IsCompleted);
            if (tasksCompleted && !objectLeft) break;

            if (objectStack.TryPop(out var obj))
            {
                world.InstantiateExact(obj);
                Callback(MathS.GetPercentage(world.ObjectCount, matches.Count, 2).ToString());
            }
        }

        var unsuccessful = tasks.Where(x => !x.Result);

        var exceptions = tasks.Select(x => x.Exception);
        if (exceptions.Any(x => x is not null))
            foreach (var ex in exceptions.Where(x => x is not null))
                Debug.LogException(ex!);

        Debug.LogError($"{unsuccessful.Count()} unsuccessful object creations...");

        sw.Stop();

        Callback($"loading complete in {sw.Elapsed.TotalMilliseconds}ms");
    }

    public bool ParseTemplateObject(string objectContent, List<WorldTemplateTypeSearchOverride> typeOverrides, ConcurrentStack<WorldObject> stack)
    {
        List<string> lines = objectContent.Split("\n", StringSplitOptions.RemoveEmptyEntries).ToList();
        lines = lines.Select(x => x.Trim().Replace('\r', '\0')).ToList();
        if (!lines[0].StartsWith("object "))
            throw new WinterException("Objects must always start with \"object\" followed by their name").WithStackTrace(lines[0]);

        string objName = lines[0].TrimStart("object".ToCharArray()).TrimEnd(':').Trim();
        if (lines.Last() != $"end {objName}")
            throw new WinterException("Objects must always end their definition the following \"end {object name}\"").WithStackTrace($"Object name: {objName}");
        if (objName.Contains(' '))
            throw new WinterException("Object names may not contain spaces").WithStackTrace($"problematic name: '{objName}'");

        WorldObject obj = new WorldObject(objName);
        var t = obj.AttachComponent<Transform>(obj);
        obj._SetTransform(t);

        lines.RemoveAt(0);
        lines.Remove(lines.Last());

        foreach (var line in lines)
        {
            if (line == "")
                continue;
            if (line.StartsWith(COMMENT))
                continue;
            if (line == Environment.NewLine)
                continue;
            string[] varassign = line.Split('=', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (varassign.Length == 1)
            {
                if (!ParseComponent(obj, varassign[0], typeOverrides))
                    return false;
                continue;
            }
            string[] vars = varassign[0].Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (varassign.Length == 2 && vars[1] == "Single")
            {

            }
            dynamic val = GetAssigningValue(varassign[1], typeOverrides, obj, vars);
            if (val is Breakout)
                return false;

            dynamic? endVar = SetVar(obj, vars, val, typeOverrides);
        }
        if (stack is not null)
            stack.Push(obj);
        else
            world.InstantiateExact(obj);
        return true;
    }
    private bool TryGetVariable(string identifyer, out WorldTemplateVariable? variable)
    {
        var a = variables.Where(x => x.Identifyer == identifyer);
        if (a.Any())
        {
            variable = a.FirstOrDefault();
            return true;
        }
        variable = null;
        return false;
    }
    private dynamic GetAssigningValue(string content, List<WorldTemplateTypeSearchOverride> typeOverrides, WorldObject obj, string[] vars)
    {
        if (TryGetVariable(content, out WorldTemplateVariable variable))
            content = variable.Value;

        if (IsPrimitive(content, out dynamic primitive))
        {
            return primitive;
        }

        int typeDefEnd = content.IndexOf("(");
        if (typeDefEnd == -1) typeDefEnd = content.Length;
        string typeDef = content[..typeDefEnd].Trim();

        if (typeDef.Contains('.'))
        {
            return GetVariable(content, typeOverrides, obj);
        }

        Type type = SelectType(TypeWorker.FindType(typeDef), typeOverrides, typeDef);

        if(type == null)
        {
            Type t = CheckEnum(content, obj, vars);
            if (t != null)
                return t;
        }

        if (type == null)
        {
            var e = new TypeNotFoundException($"Couldnt find the type of name \"{typeDef}\"");
            e.Source = "WorldTemplateLoader";
            e.SetStackTrace($"object: {obj.Name}\n\ncontent:\n{content}");
            Debug.LogException(e);
            return new Breakout();
        }

        string rest = content[typeDefEnd..].TrimStart('(').TrimEnd(')');
        string[] args = rest.Split(',', StringSplitOptions.TrimEntries);
        if (rest is "")
            args = [];
        dynamic instance = GetInstanceOf(type, typeOverrides, obj, args);

        return instance;
    }

    private Type CheckEnum(string content, WorldObject obj, string[] vars)
    {
        return typeof(Type);
    }

    private bool IsPrimitive(string data, out dynamic primitive)
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
    private dynamic GetVariable(string content, List<WorldTemplateTypeSearchOverride> typeOverrides, WorldObject obj)
    {
        string[] parts = content.Split('.', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

        dynamic cur = null;
        int i = 0;

        if (parts[0][0].IsLower())
        {
            while (true)
            {
                try
                {
                    if (world.Objects.Where(x => x is not null).Any(o => o.Name == parts[0]))
                        break;
                }
                catch { }
            }
            cur = world[parts[0]];
            i++;
        }

        try
        {
            for (int d = 0; i < parts.Length; i++)
            {
                string? part = parts[i];
                ReflectionHelper refh = ReflectionHelper.ForObject(cur is null ? obj : cur);
                cur = refh.GetValueFrom(part);
            }
        }
        catch (FieldNotFoundException)
        {
            cur = GetValue(content, typeOverrides);
            if (cur is Breakout)
            {

            }
        }

        return cur;
    }
    private dynamic GetInstanceOf(Type type, List<WorldTemplateTypeSearchOverride> typeOverrides, WorldObject obj, params string[] args)
    {
        if (obj.TryFetchComponent(type, out ObjectComponent? comp))
            return comp!;

        foreach (var c in type.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
        {
            var parameters = c.GetParameters();
            object?[] castedArgs = ValidateArgs(args, parameters, typeOverrides, obj);
            if (castedArgs.Length is 1 && castedArgs[0] is Breakout)
            {
                return castedArgs[0];
            }
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

    private object?[] ValidateArgs(string[] args, ParameterInfo[] parameters, List<WorldTemplateTypeSearchOverride> typeOverrides, WorldObject obj)
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
                if (TryGetVariable(arg, out var variable))
                {
                    if (IsPrimitive(variable!.Value, out dynamic primitive))
                    {
                        if (primitive is Breakout b)
                            return new[] { b };
                        castedArgs[i] = primitive;
                        continue;
                    }
                    else if (variable.Value.Contains('('))
                    {
                        string content = variable.Value;
                        int typeDefEnd = content.IndexOf("(");
                        if (typeDefEnd == -1) typeDefEnd = content.Length;
                        string typeDef = content[..typeDefEnd].Trim();

                        if (typeDef.Contains('.'))
                        {
                            return GetVariable(content, typeOverrides, obj);
                        }

                        Type type = SelectType(TypeWorker.FindType(typeDef), typeOverrides, typeDef);

                        if (type == null)
                        {
                            var e = new TypeNotFoundException($"Couldnt find the type of name \"{typeDef}\"");
                            e.Source = "WorldTemplateLoader";
                            e.SetStackTrace($"object: {obj.Name}\n\ncontent:\n{content}");
                            Debug.LogException(e);
                            return new[] { new Breakout() };
                        }

                        string rest = content[typeDefEnd..].TrimStart('(').TrimEnd(')');
                        string[] typeArgs = rest.Split(',', StringSplitOptions.TrimEntries);

                        dynamic instance = GetInstanceOf(type, typeOverrides, obj, typeArgs);

                        castedArgs[i] = instance;
                    }
                    else
                    {
                        castedArgs[i] = variable.Value;
                    }
                    continue;
                }

                Type t = SelectType(TypeWorker.FindType(arg), typeOverrides, arg);
                if (t is not null && obj.TryFetchComponent(t, out var comp))
                {
                    castedArgs[i] = comp;
                    continue;
                }

                if (IsPrimitive(arg, out var res))
                {
                    if (res is Breakout b)
                        return new[] { b };
                    castedArgs[i] = res;
                }
                else if (arg.Contains('.'))
                {
                    var value = GetValue(arg, typeOverrides);
                    if (value is Breakout b)
                        return new[] { b };
                    castedArgs[i] = value;
                }
                else
                    try
                    {
                        castedArgs[i] = Convert.ChangeType(arg, parameters[i].ParameterType);
                    }
                    catch
                    {
                        Debug.LogException(new WinterException($"Couldnt convert \"{arg}\" to type \"{parameters[i].ParameterType}\"")
                            .WithStackTrace("This is most likely due to a variable typo in the world template that is loading."));
                        return new[] { new Breakout() };
                    }
            }

        return castedArgs;
    }
    private Type? SelectType(Type type, List<WorldTemplateTypeSearchOverride> typeOverrides, string typeName = "")
    {
        foreach (var t in typeOverrides)
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

        type ??= TypeWorker.FindType(typeName, MonoUtils.UserDomain);
        return type;
    }
    private dynamic GetValue(string arg, List<WorldTemplateTypeSearchOverride> typeOverrides)
    {
        string[] vars = arg.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        dynamic? cur = null;
        for (int i = 0; i < vars.Length - 1; i++)
        {
            string? vari = vars[i];

            Type curType = SelectType(TypeWorker.FindType(vari), typeOverrides, vari);

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
    private dynamic? SetVar(WorldObject obj, string[] vars, dynamic value, List<WorldTemplateTypeSearchOverride> typeOverrides)
    {
        dynamic? cur = null;

        Type? t = SelectType(TypeWorker.FindType(vars[0]), typeOverrides, vars[0]);
        if (t is not null && obj.HasComponent(t))
        {
            cur = obj.FetchComponent(t);

            //for (int d = 1; d < vars.Length; d++)
            //{
            //    string? vari = vars[d];
            //    if(cur is null)
            //    ReflectionHelper refh = ReflectionHelper.ForObject(ref obj);
            //}
        }
        else
            for (int i = 0; i < vars.Length - 1; i++)
            {
                string? vari = vars[i];
                ReflectionHelper refh = ReflectionHelper.ForObject(cur is null ? obj : cur);
                cur = refh.GetValueFrom(vari);
            }

        if (cur is null)
        {
            object o = obj;
            ReflectionHelper rh = ReflectionHelper.ForObject(ref o);
            rh.SetValue(vars.Last(), value);
            return cur;
        }
        else
        {
            object o = cur;
            ReflectionHelper rh = ReflectionHelper.ForObject(ref o);
            rh.SetValue(vars.Last(), value);
            return cur;
        }

    }
    public bool ParseComponent(WorldObject obj, string componentData, List<WorldTemplateTypeSearchOverride> typeOverrides)
    {
        int typeDefEnd = componentData.IndexOf("(");
        if (typeDefEnd == -1)
            typeDefEnd = componentData.Length;

        string typeDef = componentData[..typeDefEnd].Trim();
        Type type = SelectType(TypeWorker.FindType(typeDef), typeOverrides, typeDef);
        if (type == null)
        {
            var e = new TypeNotFoundException($"Couldnt find the type of name \"{typeDef}\"");
            e.Source = "WorldTemplateLoader";
            e.SetStackTrace($"object: {obj.Name}\n\ncomponentData:\n{componentData}");
            Debug.LogException(e);
            return false;
        }
        string rest = componentData[typeDefEnd..].TrimStart('(').TrimEnd(')');
        string[] args = rest.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        dynamic instance = GetInstanceOf(type, typeOverrides, obj, args);
        if (instance is Breakout)
            return false;
        if (instance is null)
        {
            var e = new WinterException($"Couldnt create an instance of type \"{type}\"");
            Debug.LogException(e);
            return false;
        }
        obj.AttachComponent(instance);
        return true;
    }
}

internal record Breakout();