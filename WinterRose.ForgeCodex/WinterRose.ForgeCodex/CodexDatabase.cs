using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Text;
using WinterRose.ForgeCodex.Evaluation;
using WinterRose.ForgeCodex.Parsing.Ast;
using WinterRose.ForgeCodex.Storage;

namespace WinterRose.ForgeCodex;

public sealed class CodexDatabase
{
    private readonly Dictionary<string, Table> tables = new(StringComparer.OrdinalIgnoreCase);

    public StorageProvider StorageProvider { get; }

    public CodexDatabase(StorageProvider storage)
    {
        StorageProvider = storage;
        var tables = storage.ReadDatabase();

        foreach (var table in tables)
            this.tables.Add(table.Name, table);
    }

    public bool HasTable(string name) => tables.ContainsKey(name);

    public object Evaluate(string query)
    {
        var root = Codex.Parse(query);
        return CodexExecutor.Evaluate(this, root);
    }

    public Table GetTable(string name)
    {
        if (!tables.TryGetValue(name, out var table))
            throw new InvalidOperationException($"Table '{name}' does not exist.");
        return table;
    }

    public Table CreateTable(string name)
    {
        if (tables.ContainsKey(name))
            throw new InvalidOperationException($"Table '{name}' already exists.");

        var table = new Table(name);
        tables[name] = table;

        StorageProvider?.WriteTable(table);

        return table;
    }

    public void RemoveTable(string name)
    {
        if (!tables.TryGetValue(name, out var _))
            throw new InvalidOperationException($"Table '{name}' does not exist.");

        tables.Remove(name);
        StorageProvider.RemoveTable(name);
        // Persist database snapshot after removal
        StorageProvider?.WriteDatabase(this);

    }

    public IEnumerable<Table> GetTables() => tables.Values;

    public void Save()
    {
        StorageProvider.WriteDatabase(this);
    }

    internal Table Traverse(Table table, PathExpression? path)
    {
        if (path is null)
            return table;

        foreach (var segment in path.Segments)
        {
            var col = table.GetColumn(segment.FieldName);

            if (col.Metadata.ForeignTable != null)
            {
                throw new NotImplementedException("Relations not yet traversable in Take.");
                
                // code for filtering what records from the joined table are taken
                if (segment.Filter != null)
                {
                    var rows = new List<int>();
                    foreach (var i in Enumerable.Range(0, table.RowCount))
                    {
                        var ctx = new EvaluationContext(this, table, i);
                        if (EvaluateFilter(segment.Filter, ctx))
                            rows.Add(i);
                    }
                }

            }
        }

        return table;
    }

    private bool EvaluateFilter(FilterBlock filter, EvaluationContext ctx) => throw new NotImplementedException();
}
