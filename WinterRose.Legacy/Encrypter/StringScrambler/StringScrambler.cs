using System;
using System.Collections.Generic;
using System.Text;

namespace WinterRose.Encryption
{
    /// <summary>
    /// Scrambles strings with special settings settings
    /// </summary>
    public class StringScrambler
    {
        /// <summary>
        /// Represents a string literal that shows all supported characters by the scrambler
        /// </summary>
        public const string SupportedCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890@|#&^()/*-+=-`!$%:;'\"\\?.,{}\n_";
        private const char heart = '♥';
        private const char space = ' ';

        private readonly ScramblerSettings config1;
        private readonly ScramblerSettings config2;
        private readonly ScramblerSettings config3;

        /// <summary>
        /// Creates a new instance of the Encryptor class
        /// </summary>
        /// <param name="setting1"></param>
        /// <param name="setting2"></param>
        /// <param name="setting3"></param>
        public StringScrambler(ScramblerSettings setting1, ScramblerSettings setting2, ScramblerSettings setting3)
        {
            config1 = setting1;
            config2 = setting2;
            config3 = setting3;
        }

        /// <summary>
        /// encrypts the given message.
        /// </summary>
        /// <returns>The resulting string from the encrypting</returns>
        /// <param name="message"></param>
        public string Encrypt(string message)
        {
            StringBuilder result = new();

            (string main1, string wiring1) = ScramblerSettings.GetData(config1.Configuration);
            (string main2, string wiring2) = ScramblerSettings.GetData(config2.Configuration);
            (string main3, string wiring3) = ScramblerSettings.GetData(config3.Configuration);

            for (int i = 0; i < message.Length; i++)
            {
                char c = message[i];
                if (c == ' ')
                {
                    result.Append(heart);
                    continue;
                }
                if (c == '♥')
                {
                    result.Append(space);
                    continue;
                }

                char cc = ScramblerSettings.EncryptCharacter(c, main1, wiring1, config1.current);
                cc = ScramblerSettings.EncryptCharacter(cc, main2, wiring2, config2.current);
                cc = ScramblerSettings.EncryptCharacter(cc, main3, wiring3, config3.current);
                if (config1.Turn())
                    if (config2.Turn())
                        config3.Turn();

                result.Append(cc);
            }
            Reset();
            return result.ToString();
        }

        /// <summary>
        /// encrypts the given message.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="progress"></param>
        /// <param name="reportAt"></param>
        /// <returns>The resulting string from the encrypting</returns>
        public string Encrypt(string message, Action<float> progress, int reportAt = 1407900)
        {
            StringBuilder result = new();

            (string main1, string wiring1) = ScramblerSettings.GetData(config1.Configuration);
            (string main2, string wiring2) = ScramblerSettings.GetData(config2.Configuration);
            (string main3, string wiring3) = ScramblerSettings.GetData(config3.Configuration);

            for (int i = 0; i < message.Length; i++)
            {
                char c = message[i];
                if (c == ' ')
                {
                    result.Append(heart);

                    continue;
                }
                if (c == '♥')
                {
                    result.Append(space);
                    continue;
                }

                char cc = ScramblerSettings.EncryptCharacter(c, main1, wiring1, config1.current);
                cc = ScramblerSettings.EncryptCharacter(cc, main2, wiring2, config2.current);
                cc = ScramblerSettings.EncryptCharacter(cc, main3, wiring3, config3.current);
                if (config1.Turn())
                    if (config2.Turn())
                        config3.Turn();

                result.Append(cc);

                if (i is not 0 && i % reportAt == 0)
                    progress((float)MathS.GetPercentage(i, message.Length, 2));
            }
            Reset();
            return result.ToString();
        }

        /// <summary>
        /// Decrypts the given message
        /// </summary>
        /// <param name="message"></param>
        /// <returns>the resulting string from the decryption</returns>
        public string Decrypt(string? message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message), "message can not be null when decrypting");
            StringBuilder result = new();

            (string main1, string wiring1) = ScramblerSettings.GetData(config1.Configuration);
            (string main2, string wiring2) = ScramblerSettings.GetData(config2.Configuration);
            (string main3, string wiring3) = ScramblerSettings.GetData(config3.Configuration);

            foreach (char c in message)
            {
                if (c == '♥')
                {
                    result.Append(' ');
                    continue;
                }
                if (c == ' ')
                {
                    result.Append('♥');
                    continue;
                }

                char cc = config3.DecryptCharacter(c, main3, wiring3, config3.current);
                cc = config2.DecryptCharacter(cc, main2, wiring2, config2.current);
                cc = config1.DecryptCharacter(cc, main1, wiring1, config1.current);
                if (config1.Turn())
                    if (config2.Turn())
                        config3.Turn();
                result.Append(cc);
            }
            Reset();
            return result.ToString();
        }



        /// <summary>
        /// Decrypts the given message
        /// </summary>
        /// <param name="message"></param>
        /// <param name="progress"></param>
        /// <param name="reportAt"></param>
        /// <returns>the resulting string from the decryption</returns>
        public string Decrypt(string? message, Action<float> progress, int reportAt = 1407900)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message), "message can not be null when decrypting");
            StringBuilder result = new();

            (string main1, string wiring1) = ScramblerSettings.GetData(config1.Configuration);
            (string main2, string wiring2) = ScramblerSettings.GetData(config2.Configuration);
            (string main3, string wiring3) = ScramblerSettings.GetData(config3.Configuration);

            int i = 0;
            foreach (char c in message)
            {
                if (c == '♥')
                {
                    result.Append(' ');
                    continue;
                }
                if (c == ' ')
                {
                    result.Append('♥');
                    continue;
                }

                char cc = config3.DecryptCharacter(c, main3, wiring3, config3.current);
                cc = config2.DecryptCharacter(cc, main2, wiring2, config2.current);
                cc = config1.DecryptCharacter(cc, main1, wiring1, config1.current);
                if (config1.Turn())
                    if (config2.Turn())
                        config3.Turn();

                result.Append(cc);

                if (i is not 0 && i % reportAt == 0)
                    progress((float)MathS.GetPercentage(i, message.Length, 2));
                i++;
            }
            Reset();
            return result.ToString();
        }

        internal void Reset()
        {
            config1.Reset();
            config2.Reset();
            config3.Reset();
        }
    }
    /// <summary>
    /// this struct is part of the Scrambling. It is used to store the settings for the Scrambling
    /// </summary>
    public struct ScramblerSettings
    {
        internal readonly int StartingPosition;

        internal readonly ScrambleConfiguration Configuration;
        internal int current;
        internal readonly int NextTurnOver;

        /// <summary>
        /// creates a new instance of the Rotor class
        /// </summary>
        /// <param name="configuration">Selects which config is chosen for this specific rotor</param>
        /// <param name="offset">indicates at what position the rotor starts at</param>
        /// <param name="moveNext">at what rotation of this rotor should it signal the rotation of the next</param>
        public ScramblerSettings(ScrambleConfiguration configuration, int offset, int moveNext = 88)
        {
            current = 0;
            Configuration = configuration;
            NextTurnOver = moveNext;
            if (offset > Data.CONFIG1.Length)
                throw new ArgumentOutOfRangeException(nameof(offset), $"Parameter can only be between 0 and {Data.CONFIG1.Length}");
            current = StartingPosition = offset;
        }
        internal static (string main, string wiring) GetData(ScrambleConfiguration config)
        {
            string main = config switch
            {
                ScrambleConfiguration.I => Data.CONFIG1,
                ScrambleConfiguration.II => Data.CONFIG2,
                ScrambleConfiguration.III => Data.CONFIG3,
                ScrambleConfiguration.IV => Data.CONFIG4,
                ScrambleConfiguration.V => Data.CONFIG5,
                _ => throw new ArgumentException("Parameter must be I, II, III, IV, or V", nameof(config))
            };
            string wiring = config switch
            {
                ScrambleConfiguration.I => Data.CONFIG1WIRING,
                ScrambleConfiguration.II => Data.CONFIG2WIRING,
                ScrambleConfiguration.III => Data.CONFIG3WIRING,
                ScrambleConfiguration.IV => Data.CONFIG4WIRING,
                ScrambleConfiguration.V => Data.CONFIG5WIRING,
                _ => throw new ArgumentException("Parameter must be I, II, III, IV, or V", nameof(config))
            };
            return (main, wiring);
        }
        internal static char EncryptCharacter(char c, string main, string wiring, int offset)
        {
            int i;
            if ((i = main.IndexOf(c)) != -1)
            {
                int o = offset + i;
                while (o > main.Length - 1)
                    o -= main.Length;
                return wiring[o];
            }

            throw new Exception($"Character not supported by the Encrypter: {c}");
        }
        internal char DecryptCharacter(char c, string main, string wiring, int offset)
        {
            int i;
            if ((i = wiring.IndexOf(c)) != -1)
            {
                int o = i - offset;
                while (o < 0)
                    o += main.Length;
                return main[o];
            }

            throw new Exception($"Character not supported by the Encrypter: {c}");
        }
        internal bool Turn() => current++ == NextTurnOver;
        internal void Reset() => current = StartingPosition;
    }

    /// <summary>
    /// Makes for easy determaining what rotor configuration to choose from
    /// </summary>
    public enum ScrambleConfiguration
    {
        /// <summary>
        /// Represents the first configuration
        /// </summary>
        I,
        /// <summary>
        /// Represents the second configuratio
        /// </summary>
        II,
        /// <summary>
        /// Represents the third configuration
        /// </summary>
        III,
        /// <summary>
        /// Represents the fourth configuratio
        /// </summary>
        IV,
        /// <summary>
        /// Represents the fifth configuration
        /// </summary>
        V
    }

    internal static class Data
    {
        internal const string CONFIG1 = "Z`\nXy2fLu?/&4qD{6Mg^iU8Vw\"*@oCb5GmaI#}AE+zP|pJ:x-YeOF!Htv9T)hsd,;WrSN_3k%n07'l1$\\RQcj(=.BK";
        internal const string CONFIG1WIRING = "8\\T#NlnYDrW&gv`|4*XZQ$=09fjucO_wp/So1!@)KkA;G3-\"7eqix{(6^}b'J:dB5t,mV2LCIy%+EU?FM.HzRaP\nsh";

        internal const string CONFIG2 = "1}/Gq,.`c#3j;y&(WKB\"\\pXlaU)A{^@J-DLOSN_584%n'fxE\nu?tMYk*zPTwQoFsR0CZ=red!gIbH|2$i9+m67:hVv";
        internal const string CONFIG2WIRING = "\\ARs`XdD27;_/J(%BLt{,zg.yx!}?$=N0:cW8*Mk\np5jVfZlQhO@-vU1EFqe3G^mnPC\"YaI6TuiHS&4'|bK)#9wor+";

        internal const string CONFIG3 = "CvmBA!LX\\R1U_b(:HOhif%*w)W6$S.dq`pkJ38K\"IryaVD#9'PY&Fc2+;gzex^0sEQ/Nut4l}{T-=M@G5Zonj?7|,";
        internal const string CONFIG3WIRING = "f8j!s?n;pa#={}.\nS3Ryw,gm\\$UeK'BrCqzdYVDbulx/tJL:c@NAPW\n4O5|Q%*G-o^1T`7M(0+_F6&I)HhZk9E\"X2vi";

        internal const string CONFIG4 = "6N,n*vAzQ'/l5IL!`}$mesZ\\d3Xx:F+-1&k|\n78ihqW4aK;R(u#@tfY%r9wCM)bJU=.DB{SjVpyOcTgo2H^G\"P0E_?";
        internal const string CONFIG4WIRING = "O_L9%e.\\DT'GRNY7dBc^}0H:`2K,A!n|gZlFfoC=W?J#hyXx@MuU8w5&)\nEbi6*4mqS3k(ajp1rstzPI$v\";+QV/-{";

        internal const string CONFIG5 = "\"K:eW`vJ)+z_Doi%\n^u&7m@j4X8O9|=,.R6C0LB/tP'\\p5}ZMGIcf(wbF{d$gyN*T1E!H#3UV-q2YrxSh?ksnal;QA";
        internal const string CONFIG5WIRING = "=7\n&_*1u8mfL53C\\zwOoP?BN`VWyrHjX;UlQMeS|ga+p-t2D#0Y/kE9,6$AhTn(bIRs!JZcv4GK\"'F{x)q:}^i%.d@";

        internal static readonly Dictionary<string, string> SettingsConfig1;
        internal static readonly Dictionary<string, string> SettingsConfig2;
        internal static readonly Dictionary<string, string> SettingsConfig3;
        internal static readonly Dictionary<string, string> SettingsConfig4;
        internal static readonly Dictionary<string, string> SettingsConfig5;


        static Data()
        {
            SettingsConfig1 = new Dictionary<string, string>
            {
                { CONFIG1, CONFIG1WIRING }
            };

            SettingsConfig2 = new Dictionary<string, string>
            {
                { CONFIG2, CONFIG2WIRING }
            };

            SettingsConfig3 = new Dictionary<string, string>
            {
                { CONFIG3, CONFIG3WIRING }
            };

            SettingsConfig4 = new Dictionary<string, string>
            {
                { CONFIG4, CONFIG4WIRING }
            };

            SettingsConfig5 = new Dictionary<string, string>
            {
                { CONFIG5, CONFIG5WIRING }
            };
        }
    }
}