using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace WinterRose.Encryption
{
    /// <summary>
    /// Provides a methods to encrypt and decrypt messages.
    /// </summary>
    public sealed class ByteEncryptor
    {
        /// <summary>
        /// The size of the keys when using the encrypter
        /// </summary>
        private const int KEYSIZE = 256;

        public static byte[] Encrypt(byte[] data, ByteEncryptorSettings settings)
        {
            byte[] salt = Encoding.UTF8.GetBytes(settings.salt);
            byte[] iv = Encoding.UTF8.GetBytes(settings.iv);

            using var rij = new RijndaelManaged()
            {
                KeySize = KEYSIZE,
                Mode = CipherMode.CBC,
                Padding = PaddingMode.PKCS7
            };

            using var rfc = new Rfc2898DeriveBytes(settings.password, salt);
            rij.Key = rfc.GetBytes(KEYSIZE / 8);
            rij.IV = iv;

            using var ms = new MemoryStream();
            using var cs = new CryptoStream(ms, rij.CreateEncryptor(), CryptoStreamMode.Write);

            using (var bw = new BinaryWriter(cs))
            {
                bw.Write(data);
            }

            return ms.ToArray();
        }

        /// <summary>
        /// Encrypt the given message using the cridentals stored in provided settings object
        /// </summary>
        /// <param name="message"></param>
        /// <param name="settings"></param>
        /// <returns>The encrypted message</returns>
        public static string Encrypt(string message, string password, string Salt, string Iv)
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            byte[] salt = Encoding.UTF8.GetBytes(Salt);
            byte[] iv = Encoding.UTF8.GetBytes(Iv);

            using var rij = new RijndaelManaged()
            {
                KeySize = KEYSIZE,
                Mode = CipherMode.CBC,
                Padding = PaddingMode.PKCS7
            };

            using var rfc = new Rfc2898DeriveBytes(password, salt);
            rij.Key = rfc.GetBytes(KEYSIZE / 8);
            rij.IV = iv;

            using var ms = new MemoryStream();
            using var cs = new CryptoStream(ms, rij.CreateEncryptor(), CryptoStreamMode.Write);

            using (var bw = new BinaryWriter(cs))
            {
                bw.Write(data);
            }

            var bytes = ms.ToArray();
            return Convert.ToBase64String(bytes);
        }
        /// <summary>
        /// Encrypt the given message using the cridentals stored in provided settings object
        /// </summary>
        /// <param name="message"></param>
        /// <param name="settings"></param>
        /// <returns>The encrypted message</returns>
        public static string Encrypt(string message, ByteEncryptorSettings settings) => Encrypt(message, settings.password, settings.salt, settings.iv);
        /// <summary>
        /// Decrypt the given encrypted message using the cridentials stored in the provided settings object
        /// </summary>
        /// <param name="message"></param>
        /// <param name="settings"></param>
        /// <returns>The decrypted message</returns>
        public static string Decrypt(string message, string password, string Salt, string Iv)
        {
            byte[] data = Convert.FromBase64String(message);
            byte[] salt = Encoding.UTF8.GetBytes(Salt);
            byte[] iv = Encoding.UTF8.GetBytes(Iv);

            using var rij = new RijndaelManaged()
            {
                KeySize = KEYSIZE,
                Mode = CipherMode.CBC,
                Padding = PaddingMode.PKCS7
            };

            using var rfc = new Rfc2898DeriveBytes(password, salt);
            rij.Key = rfc.GetBytes(KEYSIZE / 8);
            rij.IV = iv;

            using var ms = new MemoryStream(data);
            using var cs = new CryptoStream(ms, rij.CreateDecryptor(), CryptoStreamMode.Read);

            using var br = new BinaryReader(cs);
            var bytes = br.ReadBytes(data.Length);
            return Encoding.UTF8.GetString(bytes);
        }
        public static byte[] Decrypt(byte[] data, ByteEncryptorSettings settings)
        {
            byte[] salt = Encoding.UTF8.GetBytes(settings.salt);
            byte[] iv = Encoding.UTF8.GetBytes(settings.iv);

            using var rij = new RijndaelManaged()
            {
                KeySize = KEYSIZE,
                Mode = CipherMode.CBC,
                Padding = PaddingMode.PKCS7
            };

            using var rfc = new Rfc2898DeriveBytes(settings.password, salt);
            rij.Key = rfc.GetBytes(KEYSIZE / 8);
            rij.IV = iv;

            using var ms = new MemoryStream(data);
            using var cs = new CryptoStream(ms, rij.CreateDecryptor(), CryptoStreamMode.Read);

            using var br = new BinaryReader(cs);
            var bytes = br.ReadBytes(data.Length);
            return bytes;
        }

        /// <summary>
        /// Decrypt the given encrypted message using the cridentials stored in the provided settings object
        /// </summary>
        /// <param name="message"></param>
        /// <param name="settings"></param>
        /// <returns>The decrypted message</returns>
        public static string Decrypt(string message, ByteEncryptorSettings settings) => Decrypt(message, settings.password, settings.salt, settings.iv);
    }
}

