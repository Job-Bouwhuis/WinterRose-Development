using System;
using System.Collections.Generic;

namespace WinterRose.ForgeCodex.Parsing.Ast
{
    public sealed class QueryTake
    {
        public PathExpression? Path { get; }
        public SelectionBlock Selection { get; }

        public QueryTake(PathExpression? path, SelectionBlock selection)
        {
            Path = path;
            Selection = selection ?? throw new ArgumentNullException(nameof(selection));
        }
    }
}
