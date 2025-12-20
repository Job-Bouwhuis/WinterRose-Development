using System;
using System.Collections.Generic;
using WinterRose.ForgeCodex.AutoKeys;
using WinterRose.WinterForgeSerializing.Attributes;

namespace WinterRose.ForgeCodex;

// Represents a single column in a table
public sealed class Column
{
    public string Name { get; private set; }
    public Type ColumnType { get; private set; }
    [SkipWhen("""
        #template SkipIf object actual
        {
            if actual->IsPrimaryKey == false && actual->IsUnique == false && actual->ForeignTable == null && actual->ForeignColumn == null
            {
                return true;
            }
            return false;
        }
        """)]
    public ColumnMetadata Metadata { get; private set; }

    [WFInclude]
    private List<Cell> values;

    [WFInclude]
    internal Dictionary<object, int>? primaryKeyIndex;

    private Column() => Metadata = new(); // for serialization
    public Column(string name, Type type, ColumnMetadata? metadata = null)
    {
        Name = name;
        ColumnType = type;
        Metadata = metadata ?? new ColumnMetadata();
        values = new List<Cell>();

        if (Metadata.IsPrimaryKey)
        {
            primaryKeyIndex = new Dictionary<object, int>();
        }
    }



    public void AddValue(object? val)
    {
        if (val != null)
        {
            var valType = val.GetType();
            
            if (!ColumnType.IsAssignableFrom(valType))
            {
                if (ColumnType.IsGenericType && ColumnType.GetGenericTypeDefinition() == typeof(Auto<>) &&
                    valType == ColumnType.GetGenericArguments()[0])
                {
                    // create new Auto<T> with the value
                    var autoInstance = Activator.CreateInstance(ColumnType, val);
                    val = autoInstance;
                }
                else if (ColumnType.FindInterfaces((a, b) => a == typeof(IConvertible), null).Length != 0
                    && val is IConvertible)
                {
                    val = Convert.ChangeType(val, ColumnType);
                }
                else
                {
                    throw new InvalidOperationException($"Invalid value type for column {Name}");
                }
            }
        }


        if (Metadata.IsPrimaryKey)
        {
            if (val == null)
                throw new InvalidOperationException($"Primary key column {Name} cannot contain null.");
            if (primaryKeyIndex!.ContainsKey(val))
                throw new InvalidOperationException($"Duplicate value '{val}' for primary key column {Name}.");
            primaryKeyIndex[val] = values.Count;
        }

        values.Add(new Cell(val));
    }

    public Cell this[int rowIndex] => values[rowIndex];

    public int Count => values.Count;

    public List<Cell> GetValuesCopy() => new(values);

    public IEnumerable<int> FindRows(object value)
    {
        if (Metadata.IsPrimaryKey && primaryKeyIndex != null)
        {
            if (primaryKeyIndex.TryGetValue(value, out int index))
                yield return index;
            yield break;
        }

        for (int i = 0; i < values.Count; i++)
            if (Equals(values[i].Value, value))
                yield return i;
    }

    internal void RemoveAt(int index)
    {
        if (Metadata.IsPrimaryKey && primaryKeyIndex != null)
            primaryKeyIndex.Remove(this[index].Value); 
        values.RemoveAt(index);
    }
}
