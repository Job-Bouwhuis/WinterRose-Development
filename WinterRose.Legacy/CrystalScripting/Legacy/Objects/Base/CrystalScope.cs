using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.CrystalScripting.Legacy;
using WinterRose.CrystalScripting.Legacy.Objects.Types;
using WinterRose.Serialization;

namespace WinterRose.CrystalScripting.Legacy.Objects.Base
{
    [IncludePrivateFields]
    public sealed class CrystalScope
    {
        internal bool isGlobalScope = false;

        private readonly Dictionary<string, CrystalVariable> variables;
        private readonly Dictionary<string, CrystalFunction> functions;

        [ExcludeFromSerialization]
        internal CrystalScript script;

        [ExcludeFromSerialization]
        private CrystalScope? parent;

        public IReadOnlyDictionary<string, CrystalVariable> Variables => variables;
        public IReadOnlyDictionary<string, CrystalFunction> Functions => functions;

        public CrystalScope(CrystalScope? parent = null)
        {
            variables = new Dictionary<string, CrystalVariable>();
            functions = new Dictionary<string, CrystalFunction>();
            this.parent = parent;
        }
        public CrystalScope(CrystalClass c) : this(c.Body.PublicIdeintifiers) { }
        public CrystalScope() // For serialization
        {
            variables = new();
            functions = new();
        }

        private CrystalScope(Dictionary<string, CrystalVariable> variables, Dictionary<string, CrystalFunction> functions, CrystalScope? parent = null)
        {
            this.variables = variables;
            this.functions = functions;
            this.parent = parent;
        }

        public CrystalClass GetClassIDentity(string name)
        {
            if (script is null)
                throw new CrystalError("Script Null Exception", "reference 'script' was null in CrystalScope.");

            var c = script.FindClass(name);
            return c;
        }

        public void DeclareVariable(CrystalVariable variable)
        {
            if (variables.ContainsKey(variable.Name))
                throw new CrystalError("Variable Already Exists", $"Variable '{variable.Name}' is already declared in the current scope.");

            if (script.GlobalScope.variables.ContainsKey(variable.Name))
            {
                throw new CrystalError("Cannot overrite global variable",
                    $"Variable with name '{variable.Name}' already exists as a global variable. overriting global variable is forbidden");
            }

            variables[variable.Name] = variable;
        }

        public void DeclareFunction(CrystalFunction function)
        {
            if (functions.ContainsKey(function.Name))
            {
                throw new CrystalError("Function already exists", $"Function '{function.Name}' is already declared in the current scope.");
            }
            if (script.GlobalScope.functions.ContainsKey(function.Name))
            {
                throw new CrystalError("Cannot overrite global function", $"Function with name '{function.Name}' already exists as a global function. overriting global functions is forbidden");
            }

            functions[function.Name] = function;
        }

        public bool TryGetVariable(string name, out CrystalVariable value)
        {
            if (variables.TryGetValue(name, out value))
            {
                return true;
            }

            if (parent != null)
            {
                return parent.TryGetVariable(name, out value);
            }

            return false;
        }
        public CrystalVariable? GetVariable(string name)
        {
            TryGetVariable(name, out var value);
            return value;
        }

        public bool TryGetFunction(string name, out CrystalFunction? function)
        {
            if (functions.TryGetValue(name, out function))
                return true;

            if (parent != null)
                return parent.TryGetFunction(name, out function);

            if (parent is not null)
                if (script is null)
                {
                    if (!parent.GetScript(out script))
                    {
                        throw new CrystalError("fix your script setting", "script was null");
                    }
                }

            if (!isGlobalScope)
                return script.GlobalScope.TryGetFunction(name, out function);
            return false;
        }

        private bool GetScript(out CrystalScript? script)
        {
            script = null;
            if (this.script is null)
                if (parent is not null)
                    return parent.GetScript(out script);
                else
                    return false;
            script = this.script;
            return true;
        }

        public CrystalFunction? GetFunction(string name)
        {
            TryGetFunction(name, out var function);
            return function;
        }

        public bool TryUpdateVariable(CrystalVariable value)
        {
            if (variables.ContainsKey(value.Name))
            {
                variables[value.Name] = value;
                return true;
            }

            if (parent != null)
            {
                return parent.TryUpdateVariable(value);
            }

            return false;
        }

        public bool IdentifyerExistsWithinScope(string name) => variables.ContainsKey(name) || functions.ContainsKey(name) || parent is not null && parent.IdentifyerExistsWithinScope(name);

        public CrystalScope CreateChildScope()
        {
            return new CrystalScope(this);
        }
        public CrystalScope Copy()
        {
            CrystalScope? copiedParent = null;
            if (parent is not null)
                copiedParent = parent.Copy();

            var newVars = new Dictionary<string, CrystalVariable>();
            foreach (var item in variables)
            {
                newVars.Add(item.Key, item.Value);
            }
            var newFuncs = new Dictionary<string, CrystalFunction>();
            foreach (var item in functions)
            {
                newFuncs.Add(item.Key, item.Value);
            }

            return new CrystalScope(newVars, newFuncs, copiedParent);
        }

        internal void SetParent(CrystalScope parent)
        {
            this.parent = parent;
        }

        internal void UpdateFunction(string name, CrystalFunction f)
        {
            functions[name] = f;
            f.Body.SetParent(f.Parent.Body);
        }
    }
}


