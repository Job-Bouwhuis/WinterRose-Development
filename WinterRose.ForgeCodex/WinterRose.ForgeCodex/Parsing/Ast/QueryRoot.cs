using WinterRose.ForgeCodex.Parsing.Modifiers;

namespace WinterRose.ForgeCodex.Parsing.Ast
{
    public sealed class QueryRoot
    {
        public QueryFrom From { get; }
        public QueryTake? Take { get; }
        public List<Modifier> Modifiers { get; set; } = [];

        public QueryRoot(QueryFrom from, QueryTake? take)
        {
            From = from ?? throw new ArgumentNullException(nameof(from));
            Take = take;
        }
    }
}
