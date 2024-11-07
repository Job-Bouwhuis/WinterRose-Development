using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.FileManagement;
using WinterRose.WinterThornScripting.Generation;
using WinterRose.WinterThornScripting.Interpreting;

namespace WinterRose.WinterThornScripting.Factory
{
    /// <summary>
    /// A factory class that can be used to dynamically generate thorn code.
    /// </summary>
    public static class ThornFactory
    {
        /// <summary>
        /// Creates a new class with the given values.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="functions"></param>
        /// <param name="variables"></param>
        /// <param name="constructors"></param>
        /// <returns></returns>
        public static Class Class(string name, Function[]? functions = null, Variable[]? variables = null, Constructor[]? constructors = null)
        {
            functions ??= [];
            constructors ??= [];
            variables ??= [];
            Class c = new(name, "");
            functions.Foreach(c.DeclareFunction);
            constructors.Foreach(c.DeclareConstructor);
            variables.Foreach(c.DeclareVariable);
            return c;
        }
        /// <summary>
        /// Creates a new namespace definition with the given values.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="classes"></param>
        /// <returns></returns>
        public static Namespace Namespace(string name, Class[]? classes = null)
        {
            classes ??= [];
            Namespace ns = new(name, classes);
            return ns;
        }
        /// <summary>
        /// Creates a new function definition with the given values.
        /// </summary>
        /// <param name="name">The name of the function</param>
        /// <param name="parameters">The input variables for the function</param>
        /// <param name="body"></param>
        /// <returns></returns>
        public static Function Function(string name, Block? body = null, Parameter[]? parameters = null)
        {
            parameters ??= [];
            body ??= new(null);
            Function f = new(name, "", AccessControl.Public);
            f.SetBody(body);
            return f;
        }

        public static Variable Variable(string name, string? value = null)
        {
            value ??= "null";
            Variable v = new(name, value, AccessControl.Public);
            return v;
        }

        public static Block Block(Block? parent = null)
        {
            Block b = new(parent);
            return b;
        }

        public static Block ParseStatement(this Block block, string statement)
        {
            block.Tokens.AddRange(Tokenizer.Tokenize(statement));
            return block;
        }

        internal static Namespace[]? ParseScript(string code, Block globalBlock)
        {
            // split the code into parts of the namespace without the namespace keyword
            int pos = 0;
            List<Namespace> namespaces = [];
            while (pos < code.Length)
            {
                var ns = ParseNamespace(code, ref pos, globalBlock);
                if (ns != null)
                {
                    if (ns.Name == "Break")
                        break;
                    namespaces.Add(ns);
                }
            }
            return [.. namespaces];
        }

        /// <summary>
        /// parses a namespace 
        /// </summary>
        /// <param name="code"></param>
        /// <param name="winterThorn"></param>
        /// <returns></returns>
        public static Namespace ParseNamespace(string code, WinterThorn winterThorn)
        {
            int pos = 0;
            return ParseNamespace(code, ref pos, winterThorn.GlobalBlock);
        }

        /// <summary>
        /// Parses all '.thn' files within the given directory and adds them all into the script that is returned.
        /// </summary>
        /// <param name="dir"></param>
        /// <returns></returns>
        public static WinterThorn ParseDirectory(DirectoryInfo dir)
        {
            WinterThorn result = new(dir.Name, "", "", new(1, 0, 0));

            foreach (var file in dir.GetFiles("*.thn"))
            {
                result.DefineNamespace(ParseNamespace(FileManager.Read(file.FullName), result));
            }

            return result;
        }

        private static Namespace ParseNamespace(string code, ref int pos, Block globalBlock)
        {
            // get the namespace name
            int namespaceStart = code.IndexOf("namespace", pos);
            if (namespaceStart is -1)
                return new("Break", []);

            int namespaceEnd = code.IndexOf("{", namespaceStart);
            string namespaceSection = code[namespaceStart..namespaceEnd];
            string namespaceName = namespaceSection[namespaceSection.IndexOf(" ", 9)..].Trim('\n').Trim('\r').Trim();
            pos = namespaceEnd;

            List<Class> classes = [];
            while (pos < code.Length)
            {
                var cls = ParseClass(code, ref pos, globalBlock);
                if (cls != null)
                {
                    if (cls.Name is "Break")
                        break;
                    classes.Add(cls);
                }
            }

            Namespace ns = new(namespaceName, classes);
            return ns;
        }

        private static Class? ParseClass(string code, ref int pos, Block globalBlock)
        {
            int classStart = code.IndexOf("class", pos);
            if (classStart == -1)
            {
                return new Class("Break", "");
            }
            int classEnd = code.IndexOf("{", classStart);
            string classSection = code[classStart..classEnd];
            string className = classSection[classSection.IndexOf(" ", 5)..].Trim('\n').Trim('\r').Trim();

            int classBodyEnd = GetEndOfBody(code, classStart);
            if (classBodyEnd is -1)
            {
                pos = code.Length;
                return null;
            }
            Class c = new(className, "");
            c.Block.Parent = globalBlock;

            int variableSectionStart = code.IndexOf("variables", pos);
            if (variableSectionStart is not -1 && variableSectionStart < classBodyEnd && variableSectionStart > classStart)
            {
                int variableSectionEnd = code.IndexOf("}", variableSectionStart);
                string variableSection = code[variableSectionStart..variableSectionEnd];
                var variables = ParseVariables(variableSection, c);
                pos = variableSectionEnd + 1;
                variables.Foreach(c.DeclareVariable);
            }

            ParseClassConstructors(className, code[classStart..classBodyEnd], ref pos, c, globalBlock);

            // get all the functions.
            while (pos < code.Length)
            {
                // check if we hit the end of the class.
                if (code[pos] == '}')
                {
                    pos++;
                    return c;
                }

                // get the next function.
                var function = ParseFunction(code, ref pos, classBodyEnd, c);
                if (function != null)
                {
                    if (function.Name == "Break")
                        break;
                    c.DeclareFunction(function);
                }
            }

            pos = classBodyEnd;
            return c;
        }

        private static void ParseClassConstructors(string className, string classCode, ref int pos, Class owner, Block globalBlock)
        {
            int startPos = pos;

            int classCodePos = 0;
            string[] lines = classCode.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                bool withReturnChar = lines[i].Contains('\r');
                string untrimmedLine = lines[i].TrimEnd('\r');
                string line = untrimmedLine.Trim();
                if (line.StartsWith(className))
                {
                    classCodePos += line.Length + 1;
                    if (withReturnChar) classCodePos++;

                    List<Parameter> parameters = [];
                    string parametersString = line.Replace(className, "");
                    string[] eachParameter = parametersString.Split(",", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

                    foreach (string typeAndName in eachParameter)
                    {
                        string[] split = typeAndName.Split(' ');
                        parameters.Add(new Parameter(split[1], "", split[0]));
                    }

                    int start = classCodePos;
                    int end = GetEndOfBody(classCode, classCodePos);

                    string body = classCode[start..end];
                    body = body.Replace(new string(body.TakeWhile(c => c != '{').ToArray()), "");

                    Constructor constructor = new Constructor(className, "Creates a new instance of the class", AccessControl.Public);
                    List<Token> tokens = Tokenizer.Tokenize(body);
                    Block block = new(owner.Block)
                    {
                        Tokens = tokens
                    };
                    constructor.SetBody(block);
                    constructor.SetParameters([.. parameters]);
                    owner.DeclareConstructor(constructor);
                    i = classCodePos = end;
                    continue;
                }

                classCodePos += untrimmedLine.Length + 1;
                if (withReturnChar) classCodePos++;
            }


            pos = startPos;
        }

        private static int GetEndOfBody(string code, int classStart)
        {
            // get the index of the } that closes this class. account for the fact that there may be nested code blocks that have their own }.
            int classBodyEnd = classStart;
            int openBrackets = 0;
            string start = code[classStart..];
            foreach (char c in start)
            {
                if (c == '{')
                {
                    openBrackets++;
                }
                else if (c == '}')
                {
                    openBrackets--;
                    if (openBrackets is 0)
                    {
                        classBodyEnd++;
                        break;
                    }
                }
                classBodyEnd++;
            }
            if (openBrackets is not 0)
                return -1;
            return classBodyEnd;
        }

        private static Function ParseFunction(string code, ref int pos, int classBodyEnd, Class cls)
        {
            // get access modifiers
            AccessControl access = AccessControl.Private;

            // get the section that defines the function itself. 
            int functionStart = code.IndexOf("function", pos);
            if (functionStart > classBodyEnd || functionStart is -1)
                return new Function("Break", "", AccessControl.Private);
            int functionEnd = code.IndexOf("{", functionStart);
            string functionSection = code[functionStart..functionEnd].Trim('\n').Trim('\r').Trim();

            // get the name of the function
            int nameStart = functionSection.IndexOf(" ", 8);
            int nameEnd = functionSection.IndexOf(' ', nameStart + 1);
            string name;
            if (nameEnd is -1)
                name = functionSection[nameStart..].Trim('\n').Trim('\r').Trim();
            else
                name = functionSection[nameStart..nameEnd].Trim('\n').Trim('\r').Trim();
            pos = functionEnd;

            // get the parameters of the function if there are any
            List<Parameter> parameters = [];
            if (nameEnd is not -1)
            {
                string parameterSection = functionSection[nameEnd..].Trim('\n').Trim('\r').Trim();
                parameters = ParseParameters(parameterSection);
            }


            // get the body of the function
            int bodyStart = code.IndexOf("{", functionEnd);
            int bodyEnd = GetEndOfBody(code, functionEnd) + 1;
            string bodySection = code[bodyStart..bodyEnd].Trim('\n').Trim('\r').Trim();
            Block body = new(bodySection, cls.Block);

            pos = bodyEnd;

            return new Function(name, "", access).WithParameterList([.. parameters]).WithBody(body);

        }

        private static List<Parameter> ParseParameters(string parameterSection)
        {
            if (parameterSection.Trim() == "")
            {
                return [];
            }
            List<Parameter> parameters = [];
            string[] splits = parameterSection.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            foreach (string split in splits)
            {
                string[] parts = split.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                if (parts.Length != 2)
                {
                    throw new WinterThornCompilationError(ThornError.SyntaxError, "WS-0001", $"Invalid parameter: {split}");
                }

                string name = parts[1];
                string type = parts[0];
                parameters.Add(new Parameter(name, "", type));
            }
            return parameters;
        }

        private static List<Variable> ParseVariables(string variableSection, Class c)
        {
            variableSection = variableSection.Trim();
            List<Variable> variables = [];

            string[] strings = variableSection.Split(";", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            strings[0] = strings[0][(strings[0].IndexOf('{') + 1)..].Trim('\n').Trim('\r').Trim();

            foreach (string s in strings)
            {
                var variable = ParseVariable(s);
                if (variable != null)
                {
                    variables.Add(variable);
                }
            }
            return variables;
        }

        private static Variable ParseVariable(string variableSection)
        {
            AccessControl access = AccessControl.Private;
            if (variableSection.StartsWith("public"))
            {
                access = AccessControl.Public;
            }
            else if (variableSection.StartsWith("global"))
            {
                access = AccessControl.Global;
            }
            else
            {
                // get what the access modifier is.
                int accessEnd = variableSection.IndexOf("var");
                if (accessEnd != -1)
                {
                    string accessSection = variableSection[..accessEnd];
                    if (accessSection.Trim() != "")
                        throw new WinterThornCompilationError(ThornError.SyntaxError, "WS-0001", $"Invalid access modifier: {accessSection}");
                }
            }

            int nameStart = variableSection.IndexOf("{") + 1;
            int nameEnd = variableSection.IndexOf("=", nameStart);
            if (nameEnd == -1)
            {
                string name = variableSection[nameStart..].Trim('\n').Trim('\r').Trim();
                return new Variable(name, "", access);
            }
            else
            {
                string name = variableSection[nameStart..nameEnd].Trim('\n').Trim('\r').Trim();
                int valueStart = variableSection.IndexOf("=", nameEnd) + 1;
                string value = variableSection[valueStart..].Trim('\n').Trim('\r').Trim();

                if (value.All(x => x.IsNumber()))
                {
                    return new Variable(name, "", TypeWorker.CastPrimitive<double>(value), access);
                }

                return new Variable(name, "", value, access);
            }


        }


    }
}