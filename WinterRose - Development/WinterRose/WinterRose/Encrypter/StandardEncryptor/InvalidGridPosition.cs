using System;

namespace WinterRose.Encryption;

[Serializable]
internal class InvalidGridPosition(char column, char row) : Exception($"Invalid grid position R{row}, C:{column}") { }