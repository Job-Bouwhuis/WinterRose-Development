using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.WIP.Fun
{
    /// <summary>
    /// I too like to have some fun now and then...
    /// </summary>
    public static class EnteanTranslator
    {
        private const string enteanAlphabet = "AZYXEWVTISRLPNOMQKJHUGFDCB";
        private const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";


        public static string ToEntean(string english)
        {
            StringBuilder result = new();
            
            foreach(char c in english)
            {
                if(!c.IsLetter())
                {
                    result.Append(c);
                    continue;
                }    
                
                int index = alphabet.IndexOf(c.ToUpper());
                if (c.IsUpper())
                    result.Append(enteanAlphabet[index]);
                else
                    result.Append(enteanAlphabet[index].ToLower());
            }
            return result.ToString();
        }

        public static string ToEnglish(string entean)
        {
            StringBuilder result = new();

            foreach (char c in entean)
            {
                if (!c.IsLetter())
                {
                    result.Append(c);
                    continue;
                }

                int index = enteanAlphabet.IndexOf(c.ToUpper());
                if (c.IsUpper())
                    result.Append(alphabet[index]);
                else
                    result.Append(alphabet[index].ToLower());
            }
            return result.ToString();
        }
    }
}
