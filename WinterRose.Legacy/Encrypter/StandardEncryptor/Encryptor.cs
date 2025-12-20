using System;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.Encryption;

/// <summary>
/// Provides methods to encrypt and decrypt strings.
/// </summary>
public static class Encryptor
{
    private static Grid? grid;

    public static void UseUniversalGrid(string publicKey, bool fullByteRange = false)
    {
        grid = new Grid(publicKey, fullByteRange);
    }

    public static void ClearUniversalGrid() => grid = null;

    public static string? GetAlphabet() => grid == null ? Grid.DEFAULT_ALPHABET : grid.Alphabet;

    // New core: byte-oriented encryption (works across 0..255)
    public static byte[] EncryptBytes(ReadOnlySpan<byte> messageBytes, string publicKey, ReadOnlySpan<byte> passwordBytes, int shiftAmount = 1, Action<float>? progress = null, int reportEvery = 1000)
    {
        Grid localGrid = Encryptor.grid ?? new Grid(publicKey, true); // default to full-byte-range for byte API

        int len = messageBytes.Length;
        byte[] output = new byte[len];

        // extend password using rotation as in original intent but efficient
        int pwdLen = passwordBytes.Length;
        if (pwdLen == 0) throw new ArgumentException("Password cannot be empty", nameof(passwordBytes));

        float total = len * 2f;
        float current = 0f;

        for (int i = 0; i < len; i++)
        {
            byte pwdByte = passwordBytes[i % pwdLen];

            // Optionally rotate password after completing one cycle (mimicked original behaviour)
            if (i > 0 && (i % pwdLen) == 0)
            {
                // rotate the password "left by one" for subsequent repeats:
                // instead of mutating password we simulate by using offset
                // but for simplicity we rotate the grid slightly so behaviour matches previous shifting
                // (this keeps behaviour consistent with original design)
                localGrid >>= 1;
            }

            byte plain = messageBytes[i];
            byte cipher = localGrid.GetValue(pwdByte, plain);

            if (shiftAmount != 0)
                localGrid >>= shiftAmount; // cheap offset adjustment

            output[i] = cipher;

            current++;
            if ((int)current % reportEvery == 0)
                progress?.Invoke(current / total * 100f);
        }

        return output;
    }

    public static byte[] DecryptBytes(ReadOnlySpan<byte> encryptedBytes, string publicKey, ReadOnlySpan<byte> passwordBytes, int shiftAmount = 1, Action<float>? progress = null, int reportEvery = 1000)
    {
        Grid localGrid = Encryptor.grid ?? new Grid(publicKey, true);

        int len = encryptedBytes.Length;
        byte[] output = new byte[len];

        int pwdLen = passwordBytes.Length;
        if (pwdLen == 0) throw new ArgumentException("Password cannot be empty", nameof(passwordBytes));

        float total = len * 2f;
        float current = 0f;

        for (int i = 0; i < len; i++)
        {
            byte pwdByte = passwordBytes[i % pwdLen];

            if (i > 0 && (i % pwdLen) == 0)
                localGrid >>= 1;

            byte cipher = encryptedBytes[i];
            byte plain = localGrid.GetPlain(pwdByte, cipher);

            output[i] = plain;

            if (shiftAmount != 0)
                localGrid >>= shiftAmount;

            current++;
            if ((int)current % reportEvery == 0)
                progress?.Invoke(current / total * 100f);
        }

        return output;
    }
    
    public static string Encrypt(string message, string publicKey, string password, int shiftAmount = 1, Action<float>? progress = null, int reportEvery = 1000)
    {
        byte[] messageBytes = Encoding.UTF8.GetBytes(message);
        byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
        byte[] cipherBytes = EncryptBytes(messageBytes, publicKey, passwordBytes, shiftAmount, progress, reportEvery);
        return Convert.ToBase64String(cipherBytes);
    }

    public static string Decrypt(string base64EncryptedMessage, string publicKey, string password, int shiftAmount = 1, Action<float>? progress = null, int reportEvery = 1000)
    {
        byte[] encryptedBytes = Convert.FromBase64String(base64EncryptedMessage);
        byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
        byte[] plainBytes = DecryptBytes(encryptedBytes, publicKey, passwordBytes, shiftAmount, progress, reportEvery);
        return Encoding.UTF8.GetString(plainBytes);
    }

    public static void PrintGrid()
    {
        Console.WriteLine(grid!.GetGridAsString());
    }
}
