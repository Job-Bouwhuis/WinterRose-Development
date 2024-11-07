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
public class ReadonlyCollection : CSharpClass
{
    private Variable[] values = [];

    /// <summary>
    /// The values in the collection.
    /// </summary>
    public Variable[] Values => values.ToArray();
    /// <summary>
    /// The number of values in the collection.
    /// </summary>
    public int Count => values.Length;

    /// <summary>
    /// Creates an empty collection.
    /// </summary>
    public ReadonlyCollection()
    {
    }

    public ReadonlyCollection(Variable[] args)
    {
        values = args;
    }

    public Class GetClass()
    {
        ReadonlyCollection resultclass = new();
        Class result = new(nameof(ReadonlyCollection), "") 
        { CSharpClass = resultclass };

        Function Get = new("Get", "Gets the value at the specified index.", AccessControl.Public);
        Get.CSharpFunction = (Variable index) => resultclass.Get(index);
        result.DeclareFunction(Get);

        Variable count = new("Count", "The number of values in the collection.", AccessControl.Public);
        count.Value = () => (double)resultclass.Count;
        result.DeclareVariable(count);

        return result;
    }

    public void Constructor(Variable[] args)
    {
        values = args;
    }

    public Variable Get(Variable index)
    {
        if(index.Value is not Int128 num)
            throw new WinterThornExecutionError(ThornError.InvalidType, "WT-0008", "The index must be an number.");

        if(num > int.MaxValue)
            throw new WinterThornExecutionError(ThornError.IndexOutOfRange, "WT-0009", "The index was out of range.");

        int i = int.Parse($"{num}");
        if (i < 0 || i >= values.Length)
            throw new WinterThornExecutionError(ThornError.IndexOutOfRange, "WT-0009", "The index was out of range.");

        return values[i];
    }
}
