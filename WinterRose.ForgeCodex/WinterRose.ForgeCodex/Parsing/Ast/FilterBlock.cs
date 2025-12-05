namespace WinterRose.ForgeCodex.Parsing.Ast
{
    // --- Path and Filters ---
    public sealed class FilterBlock
    {
        public IReadOnlyList<Expression> Conditions { get; }

        public FilterBlock(IReadOnlyList<Expression> conditions)
        {
            Conditions = conditions ?? Array.Empty<Expression>();
        }
    }
}
