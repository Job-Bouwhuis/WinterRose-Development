using System;
using System.Collections.Generic;
using System.Text;
using WinterRose.ForgeCodex.AutoKeys;

namespace WinterRose.ForgeCodex;

// Represents a table of columns
public sealed class Table
{
    public string Name { get; private set; }

    [WFInclude]
    private Dictionary<string, Column> columns;

    public Column PrimaryKeyColumn
    {
        get
        {
            if(field == null)
                foreach(var c in columns.Values)
                {
                    if (c.Metadata.IsPrimaryKey)
                    {
                        field = c;
                        return c;
                    }
                }
            return field ?? throw new InvalidOperationException("Table does not have a primary key column");
        }
    }

    public Table(string name)
    {
        Name = name;
        columns = new Dictionary<string, Column>(StringComparer.OrdinalIgnoreCase);
    }
    private Table() { } // for serialization

    public IEnumerable<string> ColumnNames() => columns.Keys;

    public void AddColumn(string name, Type type, ColumnMetadata? metadata = null)
    {
        if (columns.ContainsKey(name))
            throw new InvalidOperationException($"Column {name} already exists.");
        columns[name] = new Column(name, type, metadata);
    }

    public Column GetColumn(string name)
    {
        if (!columns.TryGetValue(name, out var col))
            throw new InvalidOperationException($"Column {name} not found.");
        return col;
    }

    public void AddRow(Dictionary<string, object?> rowData)
    {
        // If PK is AutoKey, generate value first
        if (PrimaryKeyColumn.ColumnType.IsAssignableTo(typeof(AutoKey)))
        {
            AutoKey key = (AutoKey)Activator.CreateInstance(PrimaryKeyColumn.ColumnType);
            rowData[PrimaryKeyColumn.Name] = KeyFactory.CreateFor(key.KeyType, PrimaryKeyColumn.GetValuesCopy().Select(cell => cell.Value).ToArray());
        }

        foreach (var kvp in columns)
        {
            if (rowData.TryGetValue(kvp.Key, out var val))
                kvp.Value.AddValue(val);
            else
                kvp.Value.AddValue(null);
        }
    }

    public int RowCount => columns.Values.FirstOrDefault()?.Count ?? 0;

    public IEnumerable<int> QueryRows(string columnName, object value)
    {
        return GetColumn(columnName).FindRows(value);
    }

    public Dictionary<string, object?> GetRow(int rowIndex)
    {
        var row = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        foreach (var kvp in columns)
        {
            row[kvp.Key] = kvp.Value[rowIndex].Value;
        }
        return row;
    }

    // Fast lookup by primary key column
    public Dictionary<string, object?>? GetRowByPrimaryKey(string columnName, object keyValue)
    {
        var col = GetColumn(columnName);
        if (!col.Metadata.IsPrimaryKey)
            throw new InvalidOperationException($"{columnName} is not a primary key column.");

        int? index = col.FindRows(keyValue).FirstOrDefault();
        if (index == null)
            return null;
        return GetRow(index.Value);
    }

    public void RemoveRow(object primaryKeyValue)
    {
        if (!PrimaryKeyColumn.Metadata.IsPrimaryKey)
            throw new InvalidOperationException("Table does not have a primary key.");

        var rowIndex = PrimaryKeyColumn.FindRows(primaryKeyValue).FirstOrDefault();
        //if (rowIndex == null)
           // throw new InvalidOperationException($"No row found with primary key value '{primaryKeyValue}'.");

        int index = rowIndex;

        foreach (var col in columns.Values)
        {
            col.RemoveAt(index);
        }
    }
}