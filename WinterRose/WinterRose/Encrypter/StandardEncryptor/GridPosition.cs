using System;

namespace WinterRose.Encryption;

internal class GridPosition(char column, char row, char value) : IEquatable<GridPosition>
{
    public char Column { get; set; } = column;
    public char Row { get; set; } = row;
    public char Value { get; set; } = value;

    public override bool Equals(object obj)
    {
        return Equals(obj as GridPosition);
    }

    public bool Equals(GridPosition other)
    {
        if (other == null)
            return false;

        return Column == other.Column && Row == other.Row;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Column, Row);
    }
}
