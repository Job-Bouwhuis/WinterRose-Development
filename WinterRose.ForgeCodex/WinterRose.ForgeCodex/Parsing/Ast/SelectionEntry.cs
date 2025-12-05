namespace WinterRose.ForgeCodex.Parsing.Ast
{
    public class SelectionEntry
    {
        public string FieldName { get; }
        public SelectionBlock? NestedSelection { get; }

        public SelectionEntry(string fieldName, SelectionBlock? nestedSelection = null)
        {
            FieldName = fieldName;
            NestedSelection = nestedSelection;
        }
    }
}
