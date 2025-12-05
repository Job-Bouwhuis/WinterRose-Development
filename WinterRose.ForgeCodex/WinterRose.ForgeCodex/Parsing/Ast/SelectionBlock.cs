namespace WinterRose.ForgeCodex.Parsing.Ast
{
    public sealed class SelectionBlock
    {
        public IReadOnlyList<SelectionEntry> Entries { get; }

        public SelectionBlock(IReadOnlyList<SelectionEntry> entries)
        {
            Entries = entries ?? Array.Empty<SelectionEntry>();
        }
    }
}
