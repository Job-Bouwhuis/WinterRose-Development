using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WinterRose.CrystalScripting.Legacy;
using WinterRose.CrystalScripting.Legacy.Interpreting;
using WinterRose.CrystalScripting.Legacy.Objects.Base;
using WinterRose.Serialization;

namespace WinterRose.CrystalScripting.Legacy.Objects.Types
{
    [DebuggerDisplay("{Name}"), IncludePrivateFields]
    public sealed class CrystalClass : CrystalType
    {
        public ulong id;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string name;

        [WFExclude]
        public CrystalScript script;

        public CrystalCodeBody Body { get; }

        public override string Name => name;

        public CrystalClass(string name, CrystalCodeBody code)
        {
            this.name = name;
            Body = code;
            id = CrystalCodeBody.GenerateBlockID();
        }
        public CrystalClass() // For serialization
        {
        }

        public bool FindFunctions(out CrystalError error)
        {
            string pattern = @"function\s+(\w+)\s*((?:\w+\s+\w+(?:\s*,\s*\w+\s+\w+)*)?)\s*[\r\n]?\s*{([\s\S]*?)\s*}";
            MatchCollection matches = Regex.Matches(string.Join("", Body.BodyTokens.Select(t => t.Lexeme)), pattern);

            foreach (Match match in matches)
            {
                string functionName = match.Groups[1].Value;
                string parameterList = match.Groups[2].Value;
                string functionBody = match.Groups[3].Value;

                // Split the parameter list into individual parameters
                string[] parameterNames = parameterList.Split(',', StringSplitOptions.RemoveEmptyEntries);

                // Trim whitespace from parameter names
                for (int i = 0; i < parameterNames.Length; i++)
                {
                    parameterNames[i] = parameterNames[i].Trim();
                }

                // Create ScriptVariable objects for each parameter
                List<CrystalVariable> parameters = new List<CrystalVariable>();
                foreach (string parameterName in parameterNames)
                {
                    if (string.IsNullOrWhiteSpace(parameterName))
                    {
                        error = new CrystalError("Invalid Function Parameter", "One or more function parameters are invalid.");
                        return false;
                    }

                    CrystalVariable parameter = new CrystalVariable(parameterName, null);
                    parameters.Add(parameter);
                }

                // Create a new CodeBody object for the function body
                List<Token> functionBodyTokens = new CrystalTokenizer(functionBody).Tokens;
                CrystalCodeBody body = new CrystalCodeBody(functionBodyTokens, Body);

                // Create a new CrystalFunction object and add it to the collection
                CrystalFunction function = new CrystalFunction(functionName, parameters.ToArray(), body, this);
                Body.PublicIdeintifiers.DeclareFunction(function);
            }

            error = null;
            return true;
        }

        public bool GetFunction(string functionName, out CrystalFunction function)
        {
            CrystalFunction? f = Body.PublicIdeintifiers.GetFunction(functionName);
            if (f is null)
            {
                function = CrystalFunction.Empty;
                return false;
            }
            function = f;
            return true;
        }

        public bool Create(out CrystalClass copy, out CrystalError error)
        {
            copy = new CrystalClass(Name, Body.Copy());

            // Copy the functions
            foreach (CrystalFunction function in Body.PublicIdeintifiers.Functions.Values)
                copy.Body.PublicIdeintifiers.DeclareFunction(function);

            error = CrystalError.NoError;
            return true;
        }

        public override object GetValue()
        {
            return this;
        }

        public override CrystalType SetValue(object value)
        {
            return CrystalVariable.Null.Type;
        }
    }
}


