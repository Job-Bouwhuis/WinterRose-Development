namespace WinterRose.Diff;

public struct ByteDiff
{
    public long PositionOriginal;
    public long PositionModified;
    public DiffType Type;
    public byte Value;

    public override string ToString()
    {
        var pos = Type == DiffType.Removed ? $"Original@{PositionOriginal}" : $"Modified@{PositionModified}";
        return $"{Type}: {Value:X2} at {pos}";
    }
}