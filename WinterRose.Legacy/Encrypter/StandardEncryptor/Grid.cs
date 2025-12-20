using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WinterRose.Encryption;

public class Grid
{
    public const string DEFAULT_ALPHABET = "ABCDEFGHIJKLMNOPQRSTUVWXYZ!@#$%^&*()_-+={}[]:;'\"\\/><,. \n\r\t\01234567890`~|♥abcdefghijklmnopqrstuvwxyz";

    // If UseFullByteRange is true we operate over 0..255 directly.
    private bool useFullByteRange;
    private byte[] alphabetBytes; // indexes -> byte value
    private Dictionary<char, int>? indexMap; // char -> index for custom alphabet (null if full byte range)
    private int rotationOffset; // cheap rotation

    internal int length => alphabetBytes.Length;

    public string Alphabet => useFullByteRange ? null! : Encoding.UTF8.GetString(alphabetBytes);

    // Create a grid that uses full 0..255 byte range
    public Grid(string publicKey, bool useFullByteRange = false)
    {
        this.useFullByteRange = useFullByteRange;
        rotationOffset = 0;

        if (useFullByteRange)
        {
            alphabetBytes = new byte[256];
            for (int i = 0; i < 256; i++)
                alphabetBytes[i] = (byte)i;
            indexMap = null;
        }
        else
        {
            // Build alphabet string from default plus public key (same reassemble behaviour).
            string assembled = ReassembleAlphabet(publicKey);
            alphabetBytes = Encoding.UTF8.GetBytes(assembled);
            indexMap = new Dictionary<char, int>(alphabetBytes.Length);
            for (int i = 0; i < alphabetBytes.Length; i++)
            {
                // Use the char representation for mapping lookup (backwards compatible)
                char c = (char)alphabetBytes[i];
                if (!indexMap.ContainsKey(c))
                    indexMap.Add(c, i);
            }
        }
    }

    private Grid()
    {
        // existing code
        alphabetBytes = Array.Empty<byte>();
        useFullByteRange = false;
        indexMap = null;
    }

    private static Grid FromAlphabet(string alphabet)
    {
        Grid grid = new();
        grid.alphabetBytes = Encoding.UTF8.GetBytes(alphabet);
        grid.rotationOffset = 0;
        grid.useFullByteRange = false;
        grid.indexMap = new Dictionary<char, int>();
        for (int i = 0; i < grid.alphabetBytes.Length; i++)
        {
            char c = (char)grid.alphabetBytes[i];
            if (!grid.indexMap.ContainsKey(c))
                grid.indexMap.Add(c, i);
        }
        return grid;
    }

    // Compute the value byte directly without storing the whole NxN grid.
    // columnByte and rowByte are byte indexes (0..255) when useFullByteRange,
    // or they are bytes representing chars in alphabetBytes when using custom alphabet.
    private byte ComputeValueByIndex(int columnIndex, int rowIndex)
    {
        int len = length;
        int valueIndex = (columnIndex - rowIndex + rotationOffset) % len;
        if (valueIndex < 0) valueIndex += len;
        return alphabetBytes[valueIndex];
    }

    // If caller provides chars (backwards-compatible), resolve them to indexes.
    internal GridPosition GetPosition(char passwordChar, char plainTextChar)
    {
        if (useFullByteRange)
        {
            // treat char as a ushort -> byte (low 8 bits). Caller should use byte API for arbitrary bytes.
            byte col = (byte)passwordChar;
            byte row = (byte)plainTextChar;
            byte val = ComputeValueByIndex(col, row);
            return new GridPosition((byte)col, (byte)row, val);
        }
        else
        {
            if (indexMap == null)
                throw new InvalidOperationException("Index map missing for custom alphabet.");

            if (!indexMap.TryGetValue(passwordChar, out int colIndex))
                throw new CharacterUnknownException(passwordChar);
            if (!indexMap.TryGetValue(plainTextChar, out int rowIndex))
                throw new CharacterUnknownException(plainTextChar);

            byte val = ComputeValueByIndex(colIndex, rowIndex);
            return new GridPosition((byte)alphabetBytes[colIndex], (byte)alphabetBytes[rowIndex], val);
        }
    }

    // New byte-oriented API for full-range operation (fast)
    internal byte GetValue(byte passwordByte, byte plainByte)
    {
        if (!useFullByteRange)
        {
            // fallback: find indexes in alphabetBytes (linear lookup), but caller should prefer full-range mode for speed
            int colIndex = Array.IndexOf(alphabetBytes, passwordByte);
            int rowIndex = Array.IndexOf(alphabetBytes, plainByte);
            if (colIndex < 0) throw new CharacterUnknownException((char)passwordByte);
            if (rowIndex < 0) throw new CharacterUnknownException((char)plainByte);
            return ComputeValueByIndex(colIndex, rowIndex);
        }
        else
        {
            // extremely fast: byte values are their own indexes
            return ComputeValueByIndex(passwordByte, plainByte);
        }
    }

    // cheap rotate: adjust offset rather than rewriting everything
    public static Grid operator >>(Grid grid, int shiftAmount)
    {
        if (grid.length == 0) return grid;
        grid.rotationOffset = (grid.rotationOffset + shiftAmount) % grid.length;
        return grid;
    }

    public static Grid operator <<(Grid grid, int shiftAmount)
    {
        if (grid.length == 0) return grid;
        grid.rotationOffset = (grid.rotationOffset - shiftAmount) % grid.length;
        return grid;
    }

    internal byte GetPlain(byte passwordByte, byte cipherByte)
    {
        int len = length;

        if (useFullByteRange)
        {
            // For full-byte-range mode indexes == byte values
            int colIndex = passwordByte;
            int valIndex = cipherByte;

            // valueIndex = (colIndex - rowIndex + rotationOffset) mod len
            // -> rowIndex = (colIndex + rotationOffset - valIndex) mod len
            int rowIndex = (colIndex + rotationOffset - valIndex) % len;
            if (rowIndex < 0) rowIndex += len;
            return (byte)rowIndex;
        }
        else
        {
            // For custom alphabets: find the column index, brute-force the row that maps to cipherByte,
            // and return the actual byte value from the alphabet (not the index).
            if (indexMap == null)
                throw new InvalidOperationException("Index map missing for custom alphabet.");

            char pc = (char)passwordByte;
            if (!indexMap.TryGetValue(pc, out int colIndex))
                throw new CharacterUnknownException(pc);

            for (int r = 0; r < len; r++)
            {
                byte candidate = ComputeValueByIndex(colIndex, r);
                if (candidate == cipherByte)
                    return alphabetBytes[r];
            }

            throw new NotFoundException();
        }
    }

    internal void ScrambleNormalAlphabet(int seed)
    {
        // If using full byte range, random shuffle the alphabetBytes
        Random random = new Random(seed);
        for (int i = alphabetBytes.Length - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            byte tmp = alphabetBytes[i];
            alphabetBytes[i] = alphabetBytes[j];
            alphabetBytes[j] = tmp;
        }

        // rebuild indexMap if needed
        if (indexMap != null)
        {
            indexMap.Clear();
            for (int i = 0; i < alphabetBytes.Length; i++)
            {
                char c = (char)alphabetBytes[i];
                if (!indexMap.ContainsKey(c))
                    indexMap.Add(c, i);
            }
        }
    }

    internal string ReassembleAlphabet(string publicKey)
    {
        // existing behaviour preserved for custom alphabet: remove duplicate chars and prepend publicKey
        string alphabet = DEFAULT_ALPHABET;
        foreach (char c in publicKey)
            alphabet = c + alphabet.Replace(c.ToString(), "");
        return alphabet;
    }

    public string GetGridAsString()
    {
        // optional debug view for custom alphabets (keeps similar formatting)
        if (useFullByteRange)
            throw new InvalidOperationException("GetGridAsString is not supported for full byte-range grids.");

        StringBuilder sb = new();
        for (int i = 0; i < length; i++)
        {
            sb.Append('\n');
            for (int j = 0; j < length; j++)
            {
                byte value = ComputeValueByIndex(j, i);
                char c = (char)value;
                if (c == '\0') sb.Append("\\0 ");
                else if (c == '\t') sb.Append("\\t ");
                else if (c == '\n') sb.Append("\\n ");
                else if (c == '\r') sb.Append("\\r ");
                else sb.Append(c + " ");
            }
        }
        return sb.ToString();
    }
}