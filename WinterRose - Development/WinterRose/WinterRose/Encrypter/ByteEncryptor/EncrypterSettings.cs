using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.Encryption
{
    /// <summary>
    /// Holds settings for the encryptor class to use. makes it easier to use these settings in multiple encryptors
    /// </summary>
    public class ByteEncryptorSettings
    {
        public byte[] Password => Encoding.UTF8.GetBytes(password);
        public byte[] Salt => Encoding.UTF8.GetBytes(salt);
        public byte[] IV => Encoding.UTF8.GetBytes(iv);

        internal string password;
        internal string salt;
        internal string iv;

        /// <summary>
        /// Creates a new instance of the EncrypterSettings class
        /// </summary>
        /// <param name="password"></param>
        /// <param name="salt"></param>
        /// <param name="iv"></param>
        [DefaultArguments("1234567890", "1234567890", "1234567890987654")]
        public ByteEncryptorSettings(string password, string salt, string iv)
        {
            this.password = password;
            this.salt = salt;
            this.iv = iv;
        }

        /// <summary>
        /// Sets the password for the encrypter to use
        /// </summary>
        /// <param name="newPassword"></param>
        public void SetPassword(string newPassword)
        {
            password = newPassword;
        }
        /// <summary>
        /// Sets the salt for the encryptor to use
        /// </summary>
        /// <param name="newSalt"></param>
        public void SetSalt(string newSalt)
        {
            salt = newSalt;
        }
        
        public void SetIV(string newIV)
        {
            if (newIV.Length != 16)
                throw new ArgumentException("IV must be 16 characters long");
            iv = newIV;
        }
    }
}
