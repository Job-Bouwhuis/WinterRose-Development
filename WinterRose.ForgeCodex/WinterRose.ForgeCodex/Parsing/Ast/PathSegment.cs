namespace WinterRose.ForgeCodex.Parsing.Ast
{
    public sealed class PathSegment
    {
        public string FieldName { get; }
        public FilterBlock? Filter { get; }

        public PathSegment(string fieldName, FilterBlock? filter = null)
        {
            FieldName = fieldName;
            Filter = filter;
        }
    }
}
