using System;
using System.Collections.Generic;
using System.Text;
using WinterRose.ForgeCodex.Evaluation;
using WinterRose.WinterForgeSerializing;
using WinterRose.WinterForgeSerializing.Workers;

namespace WinterRose.ForgeCodex.Parsing.Ast;

public sealed class CreateTableStatement : QueryFrom
{
    public string TableName { get; }
    public List<TableField> Fields { get; }

    public CreateTableStatement(string tableName, List<TableField> fields)
        : base("", null)
    {
        TableName = tableName;
        Fields = fields;
    }

    public override object? Evaluate(EvaluationContext context)
    {
        var table = context.Database.CreateTable(TableName);

        foreach (var field in Fields)
        {
            var type = WinterForgeVM.ResolveType(field.Type);
            table.AddColumn(field.Name, type, new ColumnMetadata
            {
                IsPrimaryKey = field.PrimaryKey
            });
        }
        return table;
    }
}

public sealed class TableField
{
    public string Name { get; }
    public string Type { get; }       // could be "int", "string", "list<Inventory>", etc.
    public bool PrimaryKey { get; }
    public TableField(string name, string type, bool pk)
    {
        Name = name;
        Type = type;
        PrimaryKey = pk;
    }
}
