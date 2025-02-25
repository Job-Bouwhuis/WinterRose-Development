using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.Encryption;

namespace WinterRose.FileManagement
{

    public struct DirectorySerializerSettings
    {
        /// <summary>
        /// The directory in which the encrypted or decrypted files will be placed. A null value means the files will be placed in the same directory as the source files.<br></br>
        /// Default: null
        /// </summary>
        public DirectoryInfo? DestinationDirectory { get; set; } = null;
        /// <summary>
        /// The file extension to give to the encrypted files.<br></br>
        /// Default: ".winterarchive"
        /// </summary>
        public string FileExtension { get; set; } = ".winterarchive";
        /// <summary>
        /// Whether to pack the files into a single file or not.<br></br>
        /// Default: <see langword="false"/>
        /// </summary>
        public bool UseSingleFile { get; set; } = false;

        /// <summary>
        /// This makes the encryption stronger, but also slower. It is recommended to use this only on files that are not too large. <br></br>
        /// If you are okay with waiting for the encryption to finish, and want to make absolutely sure that the files are secure, then you can use this option.
        /// <br></br><br></br>
        /// This is only used when <see cref="UseSingleFile"/> is set to <see langword="false"/>
        /// </summary>
        public bool UseStrongerEncryption { get; set; } = false;

        public StrongEncryptionSettings StrongEncryptionSettings { get; set; } = new();

        /// <summary>
        /// When <see cref="UseStrongerEncryption"/> and <see cref="UseSingleFile"/> are set to <see langword="false"/>, this is used to encrypt the files.
        /// </summary>
        public ByteEncryptorSettings ByteEncryptorSettings { get; set; } = new ByteEncryptorSettings("tyurieowqpalskdjfhgvbcnxmzm,", "pqlamznxjsiwieurhfbvgtff..", "qdrgegtf5yl;04,f");

        /// <summary>
        /// whether to use windows encryption to encrypt the files further. in this way, the files are encrypted twice and can only be decrypted on the same windows account, provided there is no hacking involved.
        /// <br></br> Default: false
        /// <br></br><br></br>
        /// Can only be used on windows.
        /// </summary>
        public bool UseWindowsEncryption
        {
            get
            {
                if (!OperatingSystem.IsWindowsVersionAtLeast(7))
                    _useWindowsEncryption = false;
                return _useWindowsEncryption;
            }
            set
            {
                if (!OperatingSystem.IsWindowsVersionAtLeast(7))
                    _useWindowsEncryption = false;
                else
                    _useWindowsEncryption = value;
            }
        }
        private bool _useWindowsEncryption = false;
        /// <summary>
        /// Not implemented yet.
        /// </summary>
        [Experimental("This_feature_is_not_implemented_yet")]
        public bool UseZipCompression { get; set; } = false;


        /// <summary>
        /// A callback function to display progress on the UI.
        /// </summary>
        public Action<string>? Progress { get; set; } = null;


        public DirectorySerializerSettings() { }
    }

    /// <summary>
    /// Settings for the stronger encryption used in <see cref="DirectorySerializerSettings"/>
    /// </summary>
    public struct StrongEncryptionSettings
    {
        /// <summary>
        /// At encryption time, report the encryption progress every x characters.
        /// <br></br> if set to -1, no reports will be made.
        /// <br></br> Default: -1
        /// </summary>
        public int ReportEvery { get; set; } = -1;

        /// <summary>
        /// The amount of shifting to use for the encryption. Higher values make the encryption stronger, but also slower.
        /// </summary>
        public int ShiftAmount { get; set; } = 2;

        /// <summary>
        /// The private password, together with the <see cref="PublcPassword"/> 
        /// </summary>
        public string PrivatePassword { get; set; } = "winter";
        /// <summary>
        /// The public key for the encryption, it is used to generate the encryption/decryption cyphering.<br></br>
        /// <b>NOTICE:</b> This should not contain any duplicate characters. and no characters that can not be found in <see cref="Encryptor.GetAlphabet()"/>
        /// </summary>
        public string PublcPassword { get; set; } = "rosaly";

        public StrongEncryptionSettings() { }
    }
}
