using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.CrystalScripting.Legacy.Interpreting;
using WinterRose.CrystalScripting.Legacy.Interpreting.Exceptions;
using WinterRose.CrystalScripting.Legacy.Objects.Base;
using WinterRose.Serialization;

namespace WinterRose.CrystalScripting.Legacy.Objects.Types
{
    [DebuggerDisplay("Name = {Name}, Arguments = {Arguments.Length}"), IncludePrivateFields]
    public class CrystalFunction : CrystalType
    {
        public ulong parentID;

        public static ref CrystalFunction Empty { get => ref empty; }
        private static CrystalFunction empty;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string name;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private CrystalVariable[] arguments;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private CrystalCodeBody body;

        [DebuggerBrowsable(DebuggerBrowsableState.Never), WFExclude]
        private CrystalClass parent;

        public CrystalVariable[] Arguments { get => arguments; set => arguments = value; }
        public CrystalCodeBody Body { get => body; set => body = value; }
        public CrystalClass Parent { get => parent; set => parent = value; }

        public bool IsValid => !string.IsNullOrWhiteSpace(name) && body.BodyTokens.Count is 0 && parent is not null;

        public override string Name => name;

        static CrystalFunction()
        {
            empty = new("", Array.Empty<CrystalVariable>(), new(new(), null), null);
        }

        public CrystalFunction(string functionName, CrystalVariable[] functionArguments, CrystalCodeBody functionBody, CrystalClass DeclaredClass)
        {
            name = functionName;
            arguments = functionArguments;
            body = functionBody;
            parent = DeclaredClass;
            parentID = 0;

            if (parent is not null)
            {
                body.SetParent(parent.Body);
                parentID = parent.id;
            }
        }

        public CrystalFunction()
        {
            name = "";
            arguments = Array.Empty<CrystalVariable>();
            parent = null;
            body = new CrystalCodeBody(new(), null);
            parentID = 0;
        }

        public virtual CrystalVariable Invoke(params CrystalVariable[] args)
        {
            if (args.Length != arguments.Length)
                throw new ArgumentException("Invalid number of arguments.");

            // Create a new variable scope for the function invocation
            CrystalScope scope = new CrystalScope(parent);
            scope.script = parent.script;
            // Set the argument values in the variable scope
            for (int i = 0; i < args.Length; i++)
            {
                args[i].Name = arguments[i].Name;
                if (!scope.TryUpdateVariable(args[i]))
                    scope.DeclareVariable(args[i]);
            }

            // Execute the tokens within the function body
            CrystalTokenInterpreter interpreter = new CrystalTokenInterpreter(ref scope);
            try
            {
                return interpreter.ExecuteTokens(body.BodyTokens);
            }
            catch (ReturnException returnException) // part of handling the return keyword
            {
                return returnException.returnValue;
            }
            finally
            {
                WinterUtils.Repeat(() => GC.Collect(), 6);
            }
        }

        internal static CrystalFunction FromFunction(CrystalFunction f, CrystalClass parent) => new(f.Name, f.Arguments, f.Body, parent);

        public override object GetValue()
        {
            throw new NotImplementedException();
        }

        public override CrystalType SetValue(object value)
        {
            return CrystalVariable.Null.Type;
        }
    }
}


