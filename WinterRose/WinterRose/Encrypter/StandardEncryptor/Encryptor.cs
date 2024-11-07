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

    /// <summary>
    /// Uses the provided public key to create a grid for repeated use. This is useful if you are going to encrypt or decrypt multiple messages with the same public key.
    /// </summary>
    /// <param name="publicKey"></param>
    public static void UseUniversalGrid(string publicKey)
    {
        grid = new Grid(publicKey);
    }

    /// <summary>
    /// Clears the universal grid. This is useful if you are done encrypting or decrypting messages and want to free up memory.
    /// </summary>
    public static void ClearUniversalGrid()
    {
        grid = null;
    }

    public static string GetAlphabet() => grid == null ? Grid.DEFAULT_ALPHABET : grid.Alphabet;


    /// <summary>
    /// Encrypts the given message using the provided public key and password.
    /// </summary>
    /// <param name="message">The message to be encrypted</param>
    /// <param name="publicKey">The public key, this key is no issue if the world would find out about this one. tho, it would weaken the encryption to those who have experience in encryption</param>
    /// <param name="password">The password used to encrypt the message, this should not be made public</param>
    /// <param name="progress">A callback function so you can display progress on your UI</param>
    /// <param name="reportEvery">every how many characters should the report callback be called</param>
    /// <returns>The encrypted message</returns>
    public static string Encrypt(char[] message, string publicKey, string password, int shiftAmount = 1, Action<float>? progress = null, int reportEvery = 1000)
    {
        Grid grid = Encryptor.grid ?? new(publicKey);

        float total = message.Length * 2;
        float current = 0;

        StringBuilder encryptedMessage = new();

        // extend the password to the length of the message by repeating it and shifting it left by one character each duplicate until it is the same length as the message
        char[] passwordArray = new char[message.Length];
        foreach (int i in message.Length)
        {
            // if we completed the password, shift it left by one character
            if (i % password.Length == 0 && i != 0)
                password = password[1..] + password[0];

            passwordArray[i] = password[i % password.Length];
            current++;
            if (current % reportEvery == 0)
                progress?.Invoke(current / total * 100f);
        }

        // encrypt each character in the message
        for (int i = 0; i < message.Length; i++)
        {
            GridPosition position = grid.GetPosition(passwordArray[i], message[i]);
            if (shiftAmount is not 0)
                _ = grid >> shiftAmount;
            encryptedMessage.Append(position.Value);

            current++;
            if (current % reportEvery == 0)
                progress?.Invoke(current / total * 100f);
        }

        return encryptedMessage.ToString();
    }

    /// <summary>
    /// Decrypts the given message using the provided public key and password. these should be the same as the ones used to encrypt the message. Otherwise, the decryption will result in nonsense.
    /// </summary>
    /// <param name="encryptedMessage">The message to be decrypted</param>
    /// <param name="publicKey">The public key, this key is no issue if the world would find out about this one. tho, it would weaken the encryption to those who have experience in encryption</param>
    /// <param name="password">The password used to encrypt the message, this should not be made public</param>
    /// <param name="progress">A callback function so you can display progress on your UI</param>
    /// <param name="reportEvery">every how many characters should the report callback be called</param>
    /// <returns>The encrypted message</returns>
    public static string Decrypt(char[] encryptedMessage, string publicKey, string password, int shiftAmount = 1, Action<float>? progress = null, int reportEvery = 1000)
    {
        Grid grid = Encryptor.grid ?? new(publicKey);
        StringBuilder decryptedMessage = new();

        float total = encryptedMessage.Length * 2;
        float current = 0;

        // extend the password to the length of the message by repeating it and shifting it left by one character each duplicate until it is the same length as the message
        char[] passwordArray = new char[encryptedMessage.Length];
        foreach (int i in encryptedMessage.Length)
        {
            // if we completed the password, shift it left by one character
            if (i % password.Length == 0 && i != 0)
                password = password[1..] + password[0];

            passwordArray[i] = password[i % password.Length];

            current++;
            if (current % reportEvery == 0)
                progress?.Invoke(current / total * 100);
        }

        // Decrypt each character in the message
        // Decrypt each character in the message
        for (int i = 0; i < encryptedMessage.Length; i++)
        {
            // Get the position of the encrypted character in the grid using the password character
            GridPosition position = grid.GetPosition(passwordArray[i], encryptedMessage[i]);

            // Shift the grid to the right by the shift amount
            if (shiftAmount is not 0)
                _ = grid >> shiftAmount;

            // Append the decrypted character to the decrypted message
            decryptedMessage.Append(position.Value);

            current++;
            if (current % reportEvery == 0)
                progress?.Invoke(current / total * 100);

            Console.Write(position.Value);
        }

        return decryptedMessage.ToString();
    }

    /// <summary>
    /// Encrypts the given message using the provided public key and password.
    /// </summary>
    /// <param name="message">The message to be encrypted</param>
    /// <param name="publicKey">The public key, this key is no issue if the world would find out about this one. tho, it would weaken the encryption to those who have experience in encryption</param>
    /// <param name="password">The password used to encrypt the message, this should not be made public</param>
    /// <param name="progress">A callback function so you can display progress on your UI</param>
    /// <param name="reportEvery">every how many characters should the report callback be called</param>
    /// <returns>The encrypted message</returns>
    public static string Encrypt(string message, string publicKey, string password, int shiftAmount = 1, Action<float>? progress = null, int reportEvery = 1000) =>
        Encrypt(message.ToCharArray(), publicKey, password, shiftAmount, progress, reportEvery);

    /// <summary>
    /// Decrypts the given message using the provided public key and password. these should be the same as the ones used to encrypt the message. Otherwise, the decryption will result in nonsense.
    /// </summary>
    /// <param name="encryptedMessage">The message to be decrypted</param>
    /// <param name="publicKey">The public key, this key is no issue if the world would find out about this one. tho, it would weaken the encryption to those who have experience in encryption</param>
    /// <param name="password">The password used to encrypt the message, this should not be made public</param>
    /// <param name="progress">A callback function so you can display progress on your UI</param>
    /// <param name="reportEvery">every how many characters should the report callback be called</param>
    /// <returns>The encrypted message</returns>
    public static string Decrypt(string encryptedMessage, string publicKey, string password, int shiftAmount = 1, Action<float>? progress = null, int reportEvery = 1000) =>
        Decrypt(encryptedMessage.ToCharArray(), publicKey, password, shiftAmount, progress, reportEvery);


    public static void PrintGrid()
    {
        Console.WriteLine(grid!.GetGridAsString());
    }
}
