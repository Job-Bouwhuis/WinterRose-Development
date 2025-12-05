namespace WinterRose.ForgeCodex.Parsing.Ast
{
    public sealed class PathExpression
    {
        public IReadOnlyList<PathSegment> Segments { get; }

        public PathExpression(IReadOnlyList<PathSegment> segments)
        {
            Segments = segments ?? Array.Empty<PathSegment>();
        }
    }
}
