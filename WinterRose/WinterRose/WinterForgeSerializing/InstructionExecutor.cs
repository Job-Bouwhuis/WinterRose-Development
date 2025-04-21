using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using System.Threading.Tasks;
using WinterRose.Reflection;
using WinterRose.Utils;

namespace WinterRose.WinterForgeSerializing
{
    public class InstructionExecutor
    {
        private static readonly ConcurrentDictionary<string, Type> typeCache = new();

        private readonly DeserializationContext context;
        private readonly Stack<int> instanceIDStack;  // Stack to manage instance IDs dynamically

        private Stack<KeyValuePair<Type, IList>> listStack = new();

        private readonly List<DispatchedReference> dispatchedReferences = [];

        public InstructionExecutor()
        {
            context = new();
            instanceIDStack = new Stack<int>();  // Initialize stack for instance IDs
        }

        public object Execute(List<Instruction> instructions)
        {
            foreach (var instruction in instructions)
            {
                switch (instruction.OpCode)
                {
                    case OpCode.DEFINE:
                        HandleDefine(instruction.Args);
                        break;
                    case OpCode.SET:
                        HandleSet(instruction.Args);
                        break;
                    case OpCode.PUSH:
                        context.ValueStack.Push(instruction.Args[0]);
                        break;
                    case OpCode.CALL:
                        HandleCall(instruction.Args[0], int.Parse(instruction.Args[1]));
                        break;
                    case OpCode.ELEMENT:
                        HandleAddElement(instruction.Args);
                        break;
                    case OpCode.LIST_START:
                        HandleCreateList(instruction.Args);
                        break;
                    case OpCode.LIST_END:
                        HandleEndList();
                        break;
                    case OpCode.END:
                        HandleEnd();
                        break;
                    case OpCode.RET:
                        return context.GetObject(int.Parse(instruction.Args[0]));
                }
            }

            return context.ObjectTable.Values;
        }

        private void HandleEndList()
        {
            IList list = listStack.Pop().Value;
            context.ValueStack.Push(list);
        }
        private void HandleCreateList(string[] args)
        {
            if(args.Length == 0)
                throw new Exception("Expected type to initialize list");
            Type itemType = ResolveType(args[0]);

            var newList = WinterUtils.CreateList(itemType);
            listStack.Push(new(itemType, newList));
        }

        private void HandleDefine(string[] args)
        {
            var typeName = args[0];
            var id = int.Parse(args[1]);
            var numArgs = int.Parse(args[2]);
            var type = ResolveType(typeName);

            List<object> constrArgs = [];
            numArgs.Repeat(i => constrArgs.Add(context.ValueStack.Pop()));
            constrArgs.Reverse();

            var instance = DynamicObjectCreator.CreateInstanceWithArguments(type, constrArgs)!;
            //var instance = ActivatorExtra.CreateInstance(type);
            context.AddObject(id, instance);

            instanceIDStack.Push(id);
        }

        private void HandleSet(string[] args)
        {
            var field = args[0];
            var rawValue = args[1];

            var instanceID = instanceIDStack.Peek();
            var target = context.GetObject(instanceID)!;

            ReflectionHelper helper = new(ref target);
            MemberData member = helper.GetMember(field);

            object? value = GetArgumentValue(rawValue, member.Type, val =>
            {
                if (member.Type.IsArray)
                {
                    if (member.Type.IsArray)
                        val = ((IList)val).GetInternalArray();
                }

                member.SetValue(ref target, val);
            });
            if (value is Dispatched)
                return; // value has been dispatched to be set later

            if(member.Type.IsArray)
                value = ((IList)value).GetInternalArray();

            member.SetValue(ref target, value);
        }

        private void Dispatch(int refID, Action<object?> method)
        {
            dispatchedReferences.Add(new(refID, method));
        }

        private record DispatchedReference(int RefID, Action<object?> method);

        private void HandleCall(string methodName, int argCount)
        {
            var args = new object[argCount];
            for (int i = argCount - 1; i >= 0; i--)
                args[i] = context.ValueStack.Pop();

            // Call method (logic to handle method calls here)
        }

        private void HandleAddElement(string[] args)
        {
            var kv = listStack.Peek();

            object instance = GetArgumentValue(args[0], kv.Key!, obj =>
                {
                    kv.Value.Add(obj);
                });
            if (instance is Dispatched)
                return; // value has been dispatched to be set later
            kv.Value.Add(instance);
        }

        private object GetArgumentValue(string arg, Type desiredType, Action<object> onDispatch)
        {
            object? value;
            switch (arg)
            {
                case "default":
                    value = ActivatorExtra.CreateInstance(desiredType)!;
                    break;

                case string s when s.StartsWith("_ref("):
                    int refID = ParseRef(s);
                    value = context.GetObject(refID);
                    if (value == null)
                    {
                        Dispatch(refID, onDispatch);
                        return new Dispatched(); // call dispatched for a later created object!
                    }
                    break;

                case string s when s.StartsWith("_stack"):
                    var stackValue = context.ValueStack.Pop();
                    if (stackValue is string ss)
                        value = ParseLiteral(ss, desiredType);
                    else
                        value = stackValue;

                    break;
                case string s when CustomValueProviderCache.Get(desiredType, out var provider):
                    value = provider._CreateObject(s, this);
                    break;
                default:
                    value = ParseLiteral(arg, desiredType);
                    break;
            }

            return value;
        }

        private void HandleEnd()
        {
            int currentID = instanceIDStack.Peek();
            object? currentObj = context.GetObject(currentID);
            for (int i = 0; i < dispatchedReferences.Count; i++)
            {
                DispatchedReference r = dispatchedReferences[i];
                if (r.RefID == currentID)
                {
                    r.method(currentObj);
                    dispatchedReferences.Remove(r);
                }
            }

            instanceIDStack.Pop();
        }

        private static object? ParseLiteral(string raw, Type target)
        {
            if (raw is "null")
                return null;
            raw = raw.Replace('.', ',');
            return TypeWorker.CastPrimitive(raw, target);
        }

        private static int ParseRef(string raw)
        {
            var inner = raw[5..^1];
            return int.Parse(inner);
        }

        private static Type ResolveType(string typeName)
        {
            ValidateKeywordType(ref typeName);
            // Check if the type is already cached
            if (typeCache.TryGetValue(typeName, out Type cachedType))
                return cachedType;

            Type resolvedType;

            // parse generic types
            if (typeName.Contains('<') && typeName.Contains('>'))
            {
                int startIndex = typeName.IndexOf('<');
                int endIndex = typeName.LastIndexOf('>');

                string baseTypeName = typeName[..startIndex];

                string genericArgsString = typeName.Substring(startIndex + 1, endIndex - startIndex - 1);

                List<string> genericArgs = ParseGenericArguments(genericArgsString);
                if (genericArgs.Count > 0)
                    baseTypeName += "`" + genericArgs.Count.ToString();

                Type baseType = TypeWorker.FindType(baseTypeName);

                Type[] resolvedGenericArgs = genericArgs
                    .Select(arg => ResolveType(arg))
                    .ToArray();

                resolvedType = baseType.MakeGenericType(resolvedGenericArgs);
            }
            else // parse non generic types
            {
                resolvedType = TypeWorker.FindType(typeName);
            }

            typeCache[typeName] = resolvedType;

            return resolvedType;
        }

        private static void ValidateKeywordType(ref string typeName)
        {
            typeName = typeName switch
            {
                "int" => "System.Int32",
                "long" => "System.Int64",
                "short" => "System.Int16",
                "byte" => "System.Byte",
                "bool" => "System.Boolean",
                "float" => "System.Single",
                "double" => "System.Double",
                "decimal" => "System.Decimal",
                "char" => "System.Char",
                "string" => "System.String",
                "object" => "System.Object",
                _ => typeName // assume it's already a CLR type or custom type
            };
        }

        // Helper method to handle the recursive parsing of generic arguments (handles nested generics)
        private static List<string> ParseGenericArguments(string args)
        {
            List<string> result = new List<string>();
            int nestingLevel = 0;
            StringBuilder currentArg = new StringBuilder();

            // Iterate through each character in the generic arguments
            for (int i = 0; i < args.Length; i++)
            {
                char c = args[i];

                if (c == ',' && nestingLevel == 0)
                {
                    // If we're at the top level and encounter a comma, we finish the current argument
                    result.Add(currentArg.ToString().Trim());
                    currentArg.Clear();
                }
                else
                {
                    // If we encounter a '<', increase nesting level
                    if (c == '<') nestingLevel++;

                    // If we encounter a '>', decrease nesting level
                    if (c == '>') nestingLevel--;

                    // Add the current character to the argument
                    currentArg.Append(c);
                }
            }

            // Add the final argument (the last part of the string)
            if (currentArg.Length > 0)
                result.Add(currentArg.ToString().Trim());

            return result;
        }

        private class Dispatched();
    }


}
