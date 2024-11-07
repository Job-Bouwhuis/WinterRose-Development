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
    [DebuggerDisplay("ScriptVariables: Count = {variables.Count}"), IncludePrivateFields]
    public sealed class CrystalVariableCollection : IEnumerable<CrystalVariable>
    {
        private ulong blockID;
        private List<CrystalVariable> variables = new List<CrystalVariable>();
        private CrystalCodeBody parentBody;

        public ulong BlockID { get => blockID; set => blockID = value; }
        public List<CrystalVariable> Variables => variables;
        public CrystalCodeBody ParentBody { get => parentBody; set => parentBody = value; }

        private CrystalVariableCollection() => blockID = 0;

        public CrystalVariableCollection(ulong blockID)
        {
            this.blockID = blockID;
        }

        public void SetVariable(CrystalVariable variable)
        {
            if (GetVariableObject(variable.Name) is CrystalVariable var)
                var.Type = variable.Type;
            else if (parentBody?.PublicIdeintifiers.GetVariable(variable.Name) is CrystalVariable parentVar)
                parentVar.Type = variable.Type;
            else
                variables.Add(variable);
        }

        public void RemoveVariable(string name)
        {
            if (variables.FirstOrDefault(v => v.Name == name) is CrystalVariable var)
            {
                variables.Remove(var);
            }
        }

        private CrystalVariable? GetVariableObject(string name) => variables.FirstOrDefault(v => v.Name == name);

        public (string Name, CrystalType Value) GetVariable(string name)
        {
            if (variables.FirstOrDefault(v => v.Name == name) is CrystalVariable var)
                return (var.Name, var.Type);
            else
                return ("", null);
        }

        public void ClearVariables() => variables.Clear();

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"BlockID: {blockID}\n");
            foreach (var item in variables)
            {
                sb.AppendLine($"\tName: {item.Name} Value: {item.Type?.GetValue()}");
            }
            return sb.ToString();
        }

        public IEnumerator<CrystalVariable> GetEnumerator()
        {
            return ((IEnumerable<CrystalVariable>)Variables).GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<CrystalVariable>)Variables).GetEnumerator();
        }

        public CrystalVariableCollection Copy()
        {
            CrystalVariableCollection collection = new CrystalVariableCollection(blockID);
            foreach (var item in variables)
            {
                collection.SetVariable(item);
            }
            return collection;
        }


    }
}

