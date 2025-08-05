using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using WinterRose.CrystalScripting.Legacy.Interpreting;
using WinterRose.CrystalScripting.Legacy.Interpreting.TokenHandlers;
using WinterRose.CrystalScripting.Legacy.Objects.Base;
using WinterRose.CrystalScripting.Legacy.Objects.Types;
using WinterRose.Legacy.Serialization;
using WinterRose.WIP.TestClasses;

namespace WinterRose.CrystalScripting.Legacy
{
    /// <summary>
    /// A class that represents a CrystalScript. This class is used to compile and execute CrystalScript code.
    /// <br></br> can be initialized with 
    /// </summary>
    [IncludePrivateFields]
    public sealed class CrystalScript
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private List<CrystalClass> classes;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private CrystalScope globalScope;

        public CrystalScope GlobalScope => globalScope;

        public List<CrystalClass> Classes => classes;
        public string Name { get => name; set => name = value; }

        private CrystalClass entryClass;
        public CrystalClass EntryClass { get => entryClass; set => entryClass = value; }

        [WFExclude]
        private List<Token> tokens;

        private bool Tokenized = false;

        public List<Token> Tokens => tokens;

        string code;
        string name;

        [DefaultArguments("no code")]
        private CrystalScript(string code)
        {
            this.code = code;
            classes = new List<CrystalClass>();
            globalScope = new();
            globalScope.isGlobalScope = true;
        }
        public static CrystalScript FromString(string name, string code)
        {
            CrystalScript script = new(code);
            script.name = name;
            return script;
        }
        public static CrystalScript? FromFile(string path, out CrystalError error)
        {
            error = CrystalError.NoError;

            FileInfo info = new(path);

            if (info.Extension is ".crystal")
            {
                CrystalScript script = new(File.ReadAllText(path));
                script.name = Path.GetFileNameWithoutExtension(path);
                return script;
            }
            else if (info.Extension is ".ccc")
            {
                string serialized = File.ReadAllText(path);
                CrystalScript script = SnowSerializer.Deserialize<CrystalScript>(serialized).Result;
                script.name = Path.GetFileNameWithoutExtension(path);

                foreach (var c in script.Classes)
                {
                    typeof(CrystalClass).GetField("script", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)!.SetValue(c, script);
                    c.Body.PublicIdeintifiers.script = script;
                    c.Body.PrivateIdentifiers.script = script;

                    foreach (var f in c.Body.PublicIdeintifiers.Functions.Values)
                    {
                        CrystalFunction func = CrystalFunction.FromFunction(f, c);

                        c.Body.PublicIdeintifiers.UpdateFunction(f.Name, func);
                    }
                }

                if (!script.FindMain(out CrystalFunction function, out var findError))
                {
                    error = new CrystalError("No Main Function", "function \"Main\" not defined. this is the entry point of your program. therefore it is required", findError);
                    return null;
                }
                script.Tokenized = true;
                script.globalScope.script = script;
                return script;
            }
            else
            {
                error = new CrystalError("Invalid source file", "Requested path does not provide a file ending in either \".crystal\" or \".ccc\"");
                return null;
            }
        }

        public CrystalVariable Execute(out CrystalError? error)
        {
            error = CrystalError.NoError;

            if (!Tokenized)
            {
                var var = CreateTokens(out error);
                var val = (CrystalBoolean)CreateTokens(out error).Type;
                if (!val.Value)
                {
                    return var;
                }
            }

            // find the entry point of the program
            if (!FindMain(out CrystalFunction function, out error))
            {
                return new CrystalVariable("function \"Main\" not defined. this is the entry point of your program. therefore it is required", CrystalType.FromObject(false));
            }

            // return the result of the main function as the result of the script
            try
            {
                return function.Invoke();
            }
            finally
            {
                WinterUtils.Repeat(() => GC.Collect(), 6);
            }
        }

        private CrystalVariable CreateTokens(out CrystalError? error)
        {
            error = CrystalError.NoError;
            // Tokenize the code
            CrystalTokenizer tokenizer = new CrystalTokenizer(code);
            tokens = tokenizer.Tokens;

            CrystalClass currentClass = null;

            // Additional processing with the tokens
            for (int tokenIndex = 0; tokenIndex < tokens.Count; tokenIndex++)
            {
                Token token = tokens[tokenIndex];

                switch (token.Lexeme)
                {
                    case "crystal":
                        {
                            string className = tokens[tokens.IndexOf(token) + 1].Lexeme;

                            List<Token> bodyTokens = CrystalClassDefinitionHandler.GetClassBodyHandler(tokenizer.Tokens, ref tokenIndex, out error);
                            CrystalClass newClass = new CrystalClass(className, new CrystalCodeBody(bodyTokens));
                            typeof(CrystalClass).GetField("script", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)!.SetValue(newClass, this);
                            newClass.Body.PublicIdeintifiers.script = this;
                            newClass.Body.PrivateIdentifiers.script = this;
                            Classes.Add(newClass);
                            currentClass = newClass;
                        }
                        break;
                    case "variables":
                        {
                            if (currentClass is null)
                            {
                                error = new CrystalError("Syntax Error", "Can not declare variables outside of a class");
                                return new CrystalVariable("Script compilation Failed", CrystalType.FromObject(false));
                            }
                            // Create a code block for the variable section
                            CrystalCodeBody variableSectionCodeBlock = ConstructCodeBlock(tokens, tokens.IndexOf(token) + 2, currentClass.Body);

                            // Handle the variable declaration
                            VariableTokenHandler.HandleVariableDeclaration(tokenizer.Tokens, token, variableSectionCodeBlock, currentClass.Body);
                        }
                        break;
                    case "function":
                        {
                            if (currentClass is null)
                            {
                                error = new CrystalError("Syntax Error", "Cannot declare a function outside of a class");
                                return new CrystalVariable("Script compilation Failed", CrystalType.FromObject(false));
                            }

                            var functionName = tokens[tokens.IndexOf(token) + 1].Lexeme;

                            // Find the opening curly brace token to determine the start of the function body
                            Token openingCurlyBrace = FindToken(tokenIndex, TokenType.LeftBrace, tokens);

                            if (openingCurlyBrace is null)
                            {
                                error = new CrystalError("Syntax Error", "Function body is missing");
                                return new CrystalVariable("Script compilation Failed", CrystalType.FromObject(false));
                            }

                            int functionBodyStartIndex = tokens.IndexOf(openingCurlyBrace);

                            // Find the closing curly brace token to determine the end of the function body
                            int nestingLevel = 0;
                            int functionBodyEndIndex = -1;

                            for (int i = functionBodyStartIndex; i < tokens.Count; i++)
                            {
                                Token currentToken = tokens[i];

                                if (currentToken.Type is TokenType.LeftBrace or TokenType.RightBrace)
                                {
                                    if (currentToken.Lexeme == "{")
                                    {
                                        nestingLevel++;
                                    }
                                    else if (currentToken.Lexeme == "}")
                                    {
                                        nestingLevel--;

                                        if (nestingLevel == 0)
                                        {
                                            functionBodyEndIndex = i;
                                            break;
                                        }
                                    }
                                }
                            }

                            if (functionBodyEndIndex == -1)
                            {
                                error = new CrystalError("Syntax Error", "Function body is incomplete");
                                return new CrystalVariable("Script compilation Failed", CrystalType.FromObject(false));
                            }

                            // Extract the function arguments
                            List<CrystalVariable> functionArguments = new List<CrystalVariable>();

                            int argumentStartIndex = tokens.IndexOf(token) + 2; // Skip the function name and the opening parenthesis
                            int argumentEndIndex = tokens.IndexOf(openingCurlyBrace);

                            for (int i = argumentStartIndex; i < argumentEndIndex; i += 3)
                            {
                                Token typeToken = tokens[i];
                                Token nameToken = tokens[i + 1];

                                CrystalType argumentType = CrystalType.TypeFromString(typeToken.Lexeme);
                                string argumentName = nameToken.Lexeme;

                                CrystalVariable argument = new CrystalVariable(argumentName, argumentType);
                                functionArguments.Add(argument);
                            }
                            var body = new CrystalCodeBody(tokens.GetRange(functionBodyStartIndex, functionBodyEndIndex - functionBodyStartIndex + 1));
                            body.PublicIdeintifiers.script = this;
                            body.PrivateIdentifiers.script = this;

                            // Create a new CrystalFunction object and assign the function arguments and body
                            CrystalFunction func = new CrystalFunction(functionName,
                                                                           functionArguments.ToArray(),
                                                                           body,
                                                                           currentClass);

                            currentClass.Body.PublicIdeintifiers.DeclareFunction(func);

                            break;
                        }
                }

                // possible expantions may be made later
            }
            return new CrystalVariable("Script compilation successful", CrystalType.FromObject(true));
        }

        private Token FindToken(int StartIndex, TokenType type, List<Token> tokens)
        {
            for (int i = StartIndex; i < tokens.Count; i++)
            {
                if (tokens[i].Type == type)
                {
                    return tokens[i];
                }
            }
            return null;
        }

        private CrystalCodeBody ConstructCodeBlock(List<Token> tokens, int startIndex, CrystalCodeBody? parent)
        {
            List<Token> codeTokens = new List<Token>();
            int braceCount = 1;
            int currentIndex = startIndex;

            while (braceCount > 0 && currentIndex < tokens.Count)
            {
                Token currentToken = tokens[currentIndex];
                codeTokens.Add(currentToken);

                if (currentToken.Type == TokenType.LeftBrace)
                {
                    braceCount++;
                }
                else if (currentToken.Type == TokenType.RightBrace)
                {
                    braceCount--;
                }

                currentIndex++;
            }

            CrystalCodeBody codeBlock = new CrystalCodeBody(codeTokens, parent);

            return codeBlock;
        }

        private bool FindMain(out CrystalFunction function, out CrystalError? error)
        {
            function = CrystalFunction.Empty;
            // Search for the 'Main' function in each class
            foreach (CrystalClass classObj in classes)
            {
                if (classObj.GetFunction("Main", out CrystalFunction mainFunction))
                {
                    function = mainFunction;
                    error = null;
                    return true;
                }
            }

            error = new CrystalError("Main Function Not Found", "The 'Main' function was not found.");
            return false;
        }

        public bool FindClasses(out CrystalError error)
        {
            error = CrystalError.NoError;
            return true;
        }

        public CrystalClass FindClass(string className)
        {
            return classes.FirstOrDefault(c => c.Name == className);
        }

        public void AddClass(CrystalClass newClass)
        {
            classes.Add(newClass);
        }

        //public SerializationResult Compile(out CrystalError? error)
        //{
        //    throw new Exception("Do not use.");
        //    error = CrystalError.NoError;
        //    CreateTokens(out error);

        //    if (error != CrystalError.NoError)
        //    {
        //        return SerializationResult.Empty;
        //    }

        //    var res = SnowSerializer.Serialize(this);
        //    if (!res.HasValue)
        //    {
        //        error = new CrystalError("Compilation Error", "Failed to compile the script");
        //    }
        //    return res;
        //}

        public CrystalVariable DecompileAndExecute(string compiledData, out CrystalError error)
        {
            throw new Exception("Do not use.");
            error = CrystalError.NoError;
            CrystalScript script = SnowSerializer.Deserialize<CrystalScript>(compiledData).Result;

            foreach (var c in script.Classes)
            {
                typeof(CrystalClass).GetField("script", System.Reflection.BindingFlags.NonPublic)!.SetValue(c, this);
                c.Body.PublicIdeintifiers.script = this;
                c.Body.PrivateIdentifiers.script = this;
                foreach (var f in c.Body.PublicIdeintifiers.Functions.Values)
                {
                    CrystalFunction func = CrystalFunction.FromFunction(f, c);

                    c.Body.PublicIdeintifiers.UpdateFunction(f.Name, func);
                }
            }

            if (!FindMain(out CrystalFunction function, out error))
            {
                return new CrystalVariable("function \"Main\" not defined. this is the entry point of your program. therefore it is required", CrystalType.FromObject(false));
            }

            // return the result of the main function as the result of the script
            try
            {
                return function.Invoke();
            }
            finally
            {
                WinterUtils.Repeat(() => GC.Collect(), 6);
            }

        }
    }
}


