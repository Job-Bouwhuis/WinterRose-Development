using System;
using System.Collections.Generic;
using System.Text;

namespace WinterRose.ForgeCodex.Parsing.Ast;

public sealed class SelectionExcept : SelectionEntry
{
    public SelectionBlock Block { get; }

    public SelectionExcept(SelectionBlock block)
        : base("*", null)
    {
        Block = block;
    }
}
