using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.CrystalScripting.Legacy.Objects.Base;
using WinterRose.CrystalScripting.Legacy.Objects.Types;
using WinterRose.Serialization;

namespace WinterRose.CrystalScripting.Legacy.Objects.Collections
{
    [DebuggerDisplay("Count = {Count}"), IncludePrivateFields]
    public sealed class CrystalFunctionCollection : IEnumerable<CrystalFunction>
    {
        List<CrystalFunction> functions = new();
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        CrystalCodeBody parentBody;

        public CrystalCodeBody Parent { get => parentBody; set => parentBody = value; }
        public bool IsEmpty => functions == null || functions.Count == 0;

        public int Count => functions.Count;

        public CrystalFunctionCollection() { }
        public CrystalFunctionCollection(CrystalCodeBody parent)
        {
            parentBody = parent;
        }

        public void AddFunction(CrystalFunction function)
        {
            if (functions == null)
                functions = new();

            functions.Add(function);
        }

        public bool GetFunction(string name, out CrystalFunction function)
        {
            if (functions == null)
            {
                function = CrystalFunction.Empty;
                return false;
            }

            foreach (CrystalFunction func in functions)
            {
                if (func.Name == name)
                {
                    function = func;
                    return true;
                }
            }

            function = CrystalFunction.Empty;
            return false;
        }

        public bool ContainsFunction(string name)
        {
            if (functions == null)
                return false;

            foreach (CrystalFunction function in functions)
            {
                if (function.Name == name)
                    return true;
            }

            return false;
        }

        public bool RemoveFunction(string name)
        {
            if (functions == null)
                return false;

            foreach (CrystalFunction function in functions)
            {
                if (function.Name == name)
                {
                    functions.Remove(function);
                    return true;
                }
            }

            return false;
        }

        public bool RemoveFunction(CrystalFunction function)
        {
            if (functions == null)
                return false;

            return functions.Remove(function);
        }

        public void Clear()
        {
            if (functions == null)
                return;

            functions.Clear();
        }

        public IEnumerator<CrystalFunction> GetEnumerator()
        {
            return functions.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return functions.GetEnumerator();
        }

        public CrystalFunctionCollection Copy()
        {
            CrystalFunctionCollection collection = new(parentBody);
            foreach (CrystalFunction function in functions)
            {
                collection.AddFunction(function);
            }

            return collection;
        }
    }
}


