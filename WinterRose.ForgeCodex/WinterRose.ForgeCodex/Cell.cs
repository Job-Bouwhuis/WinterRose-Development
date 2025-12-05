namespace WinterRose.ForgeCodex;

// Represents a single cell, can hold optional per-cell metadata
public sealed class Cell
{
    public object? Value { get; set; }
    public bool IsLocked { get; set; }
    public DateTime? LastModified { get; set; }

    public Cell(object? value)
    {
        Value = value;
        LastModified = DateTime.UtcNow;
    }
    private Cell() { } // for serialization
}

