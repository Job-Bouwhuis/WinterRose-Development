using System;

namespace WinterRose.Encryption;

[Serializable]
public class CharacterUnknownException(char c) : Exception($"The character '{c}' is not in the alphabet.") { }
