using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.WinterThornScripting.Interpreting;

namespace WinterRose.WinterThornScripting.DefaultLibrary;

/// <summary>
/// An collection used to store multiple values<Br></Br><br></br>
/// <br></br>
/// Syntax in WinterThorn:<br></br>
/// <code>
/// myCollection = []; // Creates an empty collection<br></br>
/// myCollection = [1, 2, 3]; // Creates a collection with 3 values<br></br>
/// myCollection = [1, 2, "hello", true]; // Creates a collection with 4 values<br></br>
/// <br></br>
/// // Accessing values in a collection<br></br>
/// value = myCollection[0]; // Gets the first value in the collection<br></br>
/// value = myCollection[1]; // Gets the second value in the collection<br></br>
/// </code>
/// </summary>
[SerializeAs<Collection>]
public class Collection : CSharpClass
{
    private List<Variable> values = new List<Variable>();
    /// <summary>
    /// The values in the collection.
    /// </summary>
    public Variable[] Values => values.ToArray();
    /// <summary>
    /// The number of values in the collection.
    /// </summary>
    public int Count => values.Count;

    /// <summary>
    /// Creates an empty collection.
    /// </summary>
    public Collection()
    {
    }

    public Collection(Variable[] a)
    {
        values = [.. a];
    }

    public Class GetClass()
    {
        Collection col = new();
        values.Clear();
        Class result = new(nameof(Collection), "Holds a collection of variables, and can be indexed to get a certain value.")
        { CSharpClass = col };

        Function Add = new("Add", "Adds a value to the collection.", AccessControl.Public);
        Add.CSharpFunction = (Variable value) => col.Add(value);
        result.DeclareFunction(Add);

        Function Set = new("Set", "Set a value of the collection.", AccessControl.Public);
        Set.CSharpFunction = (Variable index, Variable value) => col.Set(index, value);
        result.DeclareFunction(Set);

        Function Get = new("Get", "Gets the value at the specified index.", AccessControl.Public);
        Get.CSharpFunction = (Variable index) => col.Get(index);
        result.DeclareFunction(Get);

        Function ToString = new("ToString", "Gets the collection as a string", AccessControl.Public);
        ToString.CSharpFunction = () => col.ToString();
        result.DeclareFunction(ToString);

        Function AllSameType = new Function("AllSameType", "Checks whether all items in the array are of the same type", AccessControl.Public)
        {
            CSharpFunction = () =>
            {
                if (col.values.Count == 0)
                    return true;

                VariableType type = col.values[0].Type;
                for (int i = 1; i < col.values.Count; i++)
                {
                    Variable v = col.values[i];
                    if (v.Type != type)
                        return false;
                }
                return true;
            }
        };
        result.DeclareFunction(AllSameType);

        Variable count = new("count", "The number of values in the collection.", AccessControl.Public);
        count.Value = () => (double)col.Count;
        result.DeclareVariable(count);

        return result;
    }

    private void Set(Variable index, Variable value)
    {
        if (index.Value is not double num)
            throw new WinterThornExecutionError(ThornError.InvalidType, "WT-0008", "The index must be an number.");

        if (num > int.MaxValue)
            throw new WinterThornExecutionError(ThornError.IndexOutOfRange, "WT-0009", "The index was out of range.");

        int i = int.Parse($"{num}");
        if (i < 0 || i >= (values.Count + 1))
            throw new WinterThornExecutionError(ThornError.IndexOutOfRange, "WT-0009", "The index was out of range.");

        if (i == values.Count)
        {
            Add(value);
            return;
        }

        values[i] = value;
    }

    public void Constructor(Variable[] args)
    {
        values.AddRange(args);
    }

    /// <summary>
    /// Adds a value to the collection.
    /// </summary>
    /// <param name="value"></param>
    public void Add(Variable value)
    {
        values.Add(value);
    }

    public void AddMany(Variable[] args)
    {
        foreach (var arg in args)
            Add(arg);
    }

    /// <summary>
    /// Adds a value to the collection.
    /// </summary>
    /// <param name="value"></param>
    public void Add(object value)
    {
        values.Add(new Variable("Collection Variable", "", value));
    }

    public Variable Get(Variable index)
    {
        if (index.Value is not double num)
            throw new WinterThornExecutionError(ThornError.InvalidType, "WT-0008", "The index must be an number.");

        if (num > int.MaxValue)
            throw new WinterThornExecutionError(ThornError.IndexOutOfRange, "WT-0009", "The index was out of range.");

        int i = int.Parse($"{num}");
        if (i < 0 || i >= values.Count)
            throw new WinterThornExecutionError(ThornError.IndexOutOfRange, "WT-0009", "The index was out of range.");

        return values[i];
    }

    public override string ToString()
    {
        StringBuilder sb = new();
        sb.Append('[');
        for (int i = 0; i < values.Count; i++)
        {
            Variable v = values[i];
            sb.Append(v.Value.ToString());
            if (i < values.Count - 1)
                sb.Append(", ");
        }
        sb.Append(']');
        return sb.ToString();
    }
}
