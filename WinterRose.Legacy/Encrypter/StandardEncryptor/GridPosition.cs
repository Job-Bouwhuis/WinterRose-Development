using System;

namespace WinterRose.Encryption;

internal class GridPosition : IEquatable<GridPosition>
{
    public byte Column { get; set; }
    public byte Row { get; set; }
    public byte Value { get; set; }

    public GridPosition(byte column, byte row, byte value)
    {
        Column = column;
        Row = row;
        Value = value;
    }

    public override bool Equals(object obj) => Equals(obj as GridPosition);

    public bool Equals(GridPosition other)
    {
        if (other == null) return false;
        return Column == other.Column && Row == other.Row;
    }

    public override int GetHashCode() => HashCode.Combine(Column, Row);
}
