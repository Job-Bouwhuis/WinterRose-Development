using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace WinterRose
{
#if NET6_0_OR_GREATER
    /// <summary>
    /// Represents a mutable string
    /// </summary>
    public struct MutableString : IClearDisposable
    {
        private List<char>? characters;
        private string? lastStringver;
        private bool hasChanged;
        /// <summary>
        /// Gets whether this Mutable String is disposed
        /// </summary>
        public bool IsDisposed
        {
            get
            {
                return isDisposed;
            }
        }
        private bool isDisposed = false;
        /// <summary>
        /// Gets the length of this Mutable String
        /// </summary>
        public int Length
        {
            get => characters is null ? 0 : characters.Count;
        }


        /// <summary>
        /// Creates a new instance of the MutableString struct
        /// </summary>
        public MutableString()
        {
            hasChanged = true;
            characters = null;
            lastStringver = null;
        }
        /// <summary>
        /// Creates a new instance of the MutableString struct, using the given value as the initial value
        /// </summary>
        public MutableString(string initialValue)
        {
            characters = initialValue.ToCharList();
            lastStringver = initialValue;
            hasChanged = false;
        }
        private MutableString(List<char> characters)
        {
            this.characters = characters;
            lastStringver = null;
            hasChanged = true;
        }

        /// <summary>
        /// Creates a <see cref="MutableString"/> from a standard <see cref="string"/>
        /// </summary>
        /// <param name="s"></param>
        public static implicit operator MutableString(string s) => new(s);
        /// <summary>
        /// Creates a standard <see cref="string"/> from a <see cref="MutableString"/>
        /// </summary>
        /// <param name="s"></param>
        public static implicit operator string(MutableString s)
        {
            //return s.hasChanged ? s.characters is null ? "" : new string(s.characters.ToArray()) : s.lastStringver ?? "";
            return s.ToString();
        }
        /// <summary>
        /// Implicitly converts the MutableString into a <see cref="ReadOnlySpan{T}"/> of type <seealso cref="char"/>
        /// </summary>
        /// <param name="s"></param>
        public static implicit operator ReadOnlySpan<char>(MutableString s) => (s.characters ??= new()).ToArray();
        /// <summary>
        /// Adds the given character to this mutable string
        /// </summary>
        /// <param name="s"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        public static MutableString operator +(MutableString s, char c)
        {
            s.Append(c);
            return s;
        }
        public static MutableString operator +(MutableString s, ReadOnlySpan<char> chars) => s.Append(chars);

        /// <summary>
        /// Adds the given string to this Mutable String
        /// </summary>
        /// <param name="s"></param>
        /// <param name="text"></param>
        /// <returns>the modified Mutable String</returns>
        public static MutableString operator +(MutableString s, string text)
        {
            s.Append(text);
            return s;
        }

        /// <summary>
        /// Appends the given text to this Mutable String
        /// </summary>
        /// <param name="text"></param>
        public MutableString Append(MutableString text)
        {
            foreach (char c in text)
                (characters ??= new()).Add(c);
            hasChanged = true;
            return this;
        }
        public MutableString Append(ReadOnlySpan<char> chars)
        {
            foreach (char c in chars)
                (characters ??= new()).Add(c);
            hasChanged = true;
            return this;
        }
        /// <summary>
        /// Appends the given character to this Mutable String
        /// </summary>
        /// <param name="c"></param>
        public MutableString Append(char c)
        {
            characters ??= new();
            characters.Add(c);
            hasChanged = true;
            return this;
        }
        /// <summary>
        /// Checks if the given Mutable String starts with the given range of characters
        /// </summary>
        /// <param name="text"></param>
        /// <returns>True if the first characters of this Mutable String match the given text</returns>
        public bool StartsWith(MutableString text)
        {
            if (characters is null)
                return false;
            if (characters.Count < text.Length)
                return false;
            for (int i = 0; i < text.Length; i++)
            {
                if (characters[i] != text[i])
                    return false;
            }
            return true;
        }
        /// <summary>
        /// Checks if the given Mutable String starts with the given character
        /// </summary>
        /// <returns>True if the first character is the given character, false otherwise</returns>
        public bool StartsWith(char c) => characters is not null && characters.Count > 0 && characters[0] == c;
        /// <summary>
        /// Checks if this Mutable String ends with the specified range of characters
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public bool EndsWith(MutableString text)
        {
            if (characters is null)
                return false;
            if (characters.Count < text.Length)
                return false;
            Range range = new Range(characters.Count - text.Length, characters.Count);
            foreach (int i in range)
            {
                if (characters[i] != text[i])
                    return false;
            }
            return true;
        }
        /// <summary>
        /// Prepends the given text to this Mutable String
        /// </summary>
        /// <param name="text"></param>
        public void Prepend(MutableString text)
        {
            characters ??= new();
            text.characters ??= new();

            foreach (char c in text.characters.ReverseOrder())
                characters = characters.Prepend(c).ToList();
            hasChanged = true;
        }
        /// <summary>
        /// Splits the <see cref="MutableString"/> into multiple parts based on the given character
        /// </summary>
        /// <param name="seperator"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        private MutableString[] Split(char seperator, StringSplitOptions options = StringSplitOptions.None)
        {
            List<MutableString> result = new();
            if (characters is null)
                return Array.Empty<MutableString>();

            int index = 0;
            foreach (char c in characters)
            {
                if (c == seperator)
                {
                    index++;
                    continue;
                }
                else
                {
                    if (result.Count <= index)
                        result.Add(new MutableString());
                    result[index] += c;
                }
            }

            if (options == StringSplitOptions.RemoveEmptyEntries)
                foreach (MutableString item in result)
                    if (item.IsNullOrWhitespace())
                        result.Remove(item);

            return result.ToArray();
        }
        /// <summary>
        /// Splits the Mutable String based on the given seperator string
        /// </summary>
        /// <param name="seperator"></param>
        /// <param name="options"></param>
        /// <returns>an array of <see cref="MutableString"/> containing the data</returns>
        /// <exception cref="NotImplementedException"></exception>
        private MutableString[] Split(MutableString seperator, StringSplitOptions options = StringSplitOptions.None)
        {
            List<MutableString> result = new();
            if(characters is null)
                return Array.Empty<MutableString>();

            int index = 0;
            int lastSeperation = 0;
            int seperatorIndex = 0;
            MutableString temp = new();
            foreach(char c in characters)
            {
                temp += c;
                if (c == seperator[seperatorIndex])
                {
                    seperatorIndex++;
                }
                if(seperatorIndex == seperator.Length)
                {
                    result.Add(temp.TrimEnd(seperator));
                    temp = new();
                    seperatorIndex = 0;
                    lastSeperation = index + 1;
                }
                
                index++;
            }
            result.Add(temp);
            return result.ToArray();
        }
        /// <summary>
        /// Checks if the given character consists within this Mutable String
        /// </summary>
        /// <param name="c"></param>
        /// <returns>true if the character is contained within this Mutable String,otherwise false</returns>
        public bool Contains(char c) => characters is not null && characters.Contains(c);
        /// <summary>
        /// Checks if the given range of characters exists within this Mutable String
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public bool Contains(MutableString text)
        {
            if (characters is null)
                return false;

            int count = 0;
            for (int i = 0; i < characters.Count; i++)
            {
                char c = characters[i];
                if (count >= text.Length)
                    break;
                if (c != text[count])
                {
                    count = 0;
                    continue;
                }
                count++;
            }
            return count == text.Length;
        }
        /// <summary>
        /// Clears this MutableString
        /// </summary>
        /// <returns>The same instance with the applied changes</returns>
        public MutableString Clear()
        {
            (characters ??= new()).Clear();
            hasChanged = true;
            return this;
        }
        /// <summary>
        /// Trims the given character from the start of the sequence
        /// </summary>
        /// <param name="c"></param>
        /// <returns>The same instance with the applied changes</returns>
        public MutableString TrimStart(char c)
        {
            if (characters is not null && characters[0] == c) characters.RemoveAt(0);
            hasChanged = true;
            return this;
        }
        /// <summary>
        /// Trims the specified sequence of character from the start of this Mutable String
        /// </summary>
        /// <param name="s">The sequence of characters that will be trimmed</param>
        /// <returns>The same instance with the applied changes</returns>
        public MutableString TrimStart(MutableString s)
        {
            foreach (char c in s)
                TrimStart(c);
            return this;
        }
        /// <summary>
        /// Trims the given character from the end of the sequence
        /// </summary>
        /// <param name="c"></param>
        /// <returns>The same instance with the applied changes</returns>
        public MutableString TrimEnd(char c)
        {
            if (characters is not null && characters[characters.Count - 1] == c) characters.RemoveAt(characters.Count - 1);
            hasChanged = true;
            return this;
        }
        /// <summary>
        /// Trims the specified sequence of characters from the end of this Mutable String
        /// </summary>
        /// <param name="s">The sequence of characters that will be trimmed</param>
        /// <returns>The same instance with the applied changes</returns>
        public MutableString TrimEnd(MutableString s)
        {
            foreach (char c in s.ReverseOrder())
                TrimEnd(c);
            return this;    
        }
        /// <summary>
        /// Trims all whitespaces before and after any text
        /// </summary>
        /// <returns>The same instance with the applied changes</returns>
        public MutableString Trim()
        {
            MutableString result = this;
            result.TrimStart();
            return result.TrimEnd();
        }
        /// <summary>
        /// Trims all whitespaces from the start of this sequence
        /// </summary>
        /// <returns>The same instance with the applied changes</returns>
        public MutableString TrimStart()
        {
            if (characters is null)
                return this;
            foreach (char c in characters)
            {
                if (c != ' ')
                    break;
                characters.RemoveAt(0);
            }
            hasChanged = true;
            return this;
        }
        /// <summary>
        /// Trims all whitespaces from the end of this sequence
        /// </summary>
        /// <returns>The same instance with the applied changes</returns>
        public MutableString TrimEnd()
        {
            if (characters is null)
                return this;
            for (int i = characters.Count - 1; i >= 0; i--)
            {
                if (characters[i] != ' ')
                    break;
                characters.RemoveAt(i);
            }
            hasChanged = true;
            return this;
        }
        /// <summary>
        /// Converts the sequence of characters into a base64 string. <br></br><b>Notice:</b> this creates a normal string instance and not a <see cref="MutableString"/>;
        /// </summary>
        /// <returns></returns>
        public string Base64Encode() => characters is null ? "" : Convert.ToBase64String(characters.Select(c => (byte)c).ToArray());
        /// <summary>
        /// Converts the sequence of base64 characters into a normal string. <br></br><b>Notice:</b> this creates a normal string instance and not a <see cref="MutableString"/>;
        /// </summary>
        /// <returns></returns>
        public string Base64Decode() => characters is null ? "" : Encoding.UTF8.GetString(Convert.FromBase64String(this));
        /// <summary>
        /// Indexes and finds the character at the specified index
        /// </summary>
        /// <param name="index"></param>
        /// <returns>the found character</returns>
        /// <exception cref="IndexOutOfRangeException">thrown if the index is out of range</exception>
        public char this[int index]
        {
            get
            {
                if (characters == null) return '\0';
                if (index > characters.Count) throw new IndexOutOfRangeException($"Index of {index} is too large to fetch character. There arent enough characters in this Mutable String");
                if (index < 0) throw new IndexOutOfRangeException("Index can not be smaller than 0");
                return characters[index];
            }
            set
            {
                if (index < 0) throw new IndexOutOfRangeException("Index can not be smaller than 0");

                characters ??= new();
                if (index > characters.Count)
                    characters.Add(value);
                else
                    characters[index] = value;
                hasChanged = true;
            }
        }
        /// <summary>
        /// Gets a range of characters of this Mutable string.
        /// </summary>
        /// <param name="index"></param>
        /// <returns>A new Mutable string containing the characters of the given range</returns>
        public MutableString this[Range index]
        {
            get
            {
                if (characters is null) return new MutableString();
                if (index.Start.Value < 0) throw new IndexOutOfRangeException("Start index can not be smaller than 0");
                if (index.End.Value + 1 > characters.Count || index.Start.Value > characters.Count)
                    throw new IndexOutOfRangeException("Indexes can not be larger than the amount of characters in this Mutable String");

                var result = new MutableString();
                var range = new Range(index.Start.Value, index.End.Value == 0 ? characters.Count : index.End.Value + 1);
                foreach (int i in range)
                    result += characters[i];
                return result;
            }
        }
        /// <summary>
        /// Exactly reverses the order of this Mutable String. 
        /// </summary>
        /// <returns>The same instance with the applied changes</returns>
        public MutableString ReverseOrder()
        {
            if (characters is null)
                return this;
            characters = characters.ReverseOrder();
            hasChanged = true;
            return this;
        }
        /// <summary>
        /// Gets the index of the specified character.
        /// </summary>
        /// <param name="c"></param>
        /// <returns>The index at which the character given exists. or 0 if it does not exist in this Mutable String</returns>
        public int IndexOf(char c) => characters is null ? 0 : characters.IndexOf(c);
        /// <summary>
        /// Checks if this Mutable string is null, empty, or consists of only whitespace characters
        /// </summary>
        /// <returns>True if this Mutable string is null, empty, or consists of only whitespace characters, otherwise false</returns>
        public bool IsNullOrWhitespace() => characters is null || characters.Count == 0 || characters.All(c => char.IsWhiteSpace(c));
        /// <summary>
        /// Checks if this Mutable string is null or empty
        /// </summary>
        /// <returns>True if this Mutable string is null or empty, otherwise false</returns>
        public bool IsNullOrEmpty() => characters is null || characters.Count == 0;
        /// <summary>
        /// Gets the enumerator that hold all characters
        /// </summary>
        /// <returns></returns>
        public IEnumerator GetEnumerator()
        {
            characters ??= new();
            return characters.GetEnumerator();
        }
        /// <summary>
        /// Creates a standard <see cref="string"/> interpretation of this Mutable String
        /// </summary>
        /// <returns>A <see cref="string"/> representation of this Mutable String</returns>
        public override string ToString()
        {
            try
            {
                if (hasChanged)
                {
                    if (characters is null)
                    {
                        return "";
                    }
                    return new string(characters.ToArray());
                }
                else
                {
                    return lastStringver ?? "";
                }
            }
            finally
            {
                GC.Collect();
            }
        }
        /// <summary>
        /// Disposes this Mutable String
        /// </summary>
        public void Dispose()
        {
            characters = null;
            lastStringver = null;
            isDisposed = true;
        }
    }
#endif
}