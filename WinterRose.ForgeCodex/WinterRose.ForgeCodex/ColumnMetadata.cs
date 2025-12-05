namespace WinterRose.ForgeCodex;

// Represents per-column metadata
public sealed class ColumnMetadata
{
    public bool IsPrimaryKey { get; set; }
    public bool IsUnique { get; set; }

    // Optional foreign key info
    public string? ForeignTable { get; set; }
    public string? ForeignColumn { get; set; }
}

