using WinterRose.WinterForgeSerializing.Attributes;

namespace WinterRose.ForgeCodex;

// Represents per-column metadata
public sealed class ColumnMetadata
{

    [SkipWhen(false)]
    public bool IsPrimaryKey { get; set; }

    [SkipWhen(false)]
    public bool IsUnique { get; set; }

    [SkipWhen(null)]
    public string? ForeignTable { get; set; }

    [SkipWhen(null)]
    public string? ForeignColumn { get; set; }
}

