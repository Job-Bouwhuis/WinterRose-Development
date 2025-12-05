using System;
using System.Collections.Generic;
using System.Text;

namespace WinterRose.ForgeCodex.Parsing.Ast;

public sealed class AssignmentEntry
{
    public string Field { get; }
    public Expression Value { get; }

    public AssignmentEntry(string field, Expression value)
    {
        Field = field;
        Value = value;
    }
}
