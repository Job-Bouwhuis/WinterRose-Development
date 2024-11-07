using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WinterRose.Encryption
{
    internal class Grid
    {
        internal const string DEFAULT_ALPHABET = "ABCDEFGHIJKLMNOPQRSTUVWXYZ!@#$%^&*()_-+={}[]:;'\"\\/><,. \n\r\t\01234567890`~|♥abcdefghijklmnopqrstuvwxyz";
        private string alphabetToUse = DEFAULT_ALPHABET; // ☺☻♦♣♠•◘○◙♂♀♪♫-☼►◄↕‼¶§▬↨↑↓→←∟↔▲▼

        internal int length => alphabetToUse.Length;

        /// <summary>
        /// Gets the alphabet used in the grid
        /// </summary>
        public string Alphabet => alphabetToUse;

        private Dictionary<(char column, char row), GridPosition> grid;

        /// <summary>
        /// Creates a new grid with the given public key. this public is used to generate the grid which is then used to encrypt and decrypt messages.
        /// </summary>
        /// <param name="publicKey">The public key used to generate this grid</param>
        /// The default characters will always be present regardless of this number</param>
        public Grid(string publicKey)
        {
            grid = new();

            // make sure the public key is uppercase, and does not contain any duplicate letters
            publicKey = new string(publicKey.Distinct().ToArray());

            alphabetToUse = ReassembleAlphabet(publicKey);

            // fill the grid with the alphabet, where on each row the alphabet is shifted by one letter to the right
            for (int i = 0; i < length; i++)
            {
                char row = alphabetToUse[i];
                for (int j = 0; j < length; j++)
                {
                    char column = alphabetToUse[j];
                    char value = alphabetToUse[(j - i + length) % length]; // Shift left by one
                    grid.Add((column, row), new GridPosition(column, row, value));
                }
            }
        }
        private Grid()
        {
            grid = new();
        }
        private static Grid FromAlphabet(string alphabet)
        {
            Grid grid = new();
            grid.alphabetToUse = alphabet;
            return grid;
        }

        private GridPosition? GetValue(char column, char row)
        {
            if (grid.TryGetValue((column, row), out GridPosition? foundPosition))
            {
                return foundPosition;
            }
            else
            {
                return null;
            }
        }

        internal GridPosition GetPosition(char passwordChar, char plainTextChar)
        {
            //if ((!alphabetToUse.Contains(' ')) && char.IsWhiteSpace(plainTextChar) && char.IsWhiteSpace(passwordChar))
            //    return new GridPosition(' ', ' ', ' '); // return a space if the character is a space

            //// do error checking if the column or row is not in the alphabet
            //if (!alphabetToUse.Contains(passwordChar))
            //    throw new CharacterUnknownException(passwordChar);
            //if (!alphabetToUse.Contains(plainTextChar))
            //    throw new CharacterUnknownException(plainTextChar);

            // find the position in the grid where the column and row match the password and plain text characters
            var result = GetValue(passwordChar, plainTextChar) ?? throw new NotFoundException();
            return result;
        }
        internal void ScrambleNormalAlphabet(int seed)
        {
            Random random = new Random(seed);
            alphabetToUse = new string(alphabetToUse.OrderBy(x => random.Next()).ToArray());
        }
        internal string ReassembleAlphabet(string publicKey)
        {
            string alphabet = alphabetToUse;

            // remove all the letters in the public key from the alphabet
            foreach (char c in publicKey)
                alphabet = alphabet.Replace(c.ToString(), "");

            // reassemble the alphabet with the public key at the beginning
            return publicKey + alphabet;
        }

        /// <summary>
        /// Gets the grid as a string
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public string GetGridAsString()
        {
            StringBuilder sb = new();
            for (int i = 0; i < length; i++)
            {
                char row = alphabetToUse[i];
                sb.Append('\n');
                for (int j = 0; j < length; j++)
                {
                    char column = alphabetToUse[j];
                    GridPosition position = GetValue(column, row) ?? throw new InvalidGridPosition(column, row);
                    if (position.Value == '\0')
                    {
                        sb.Append("\\0 ");
                    }
                    else if (position.Value is '\t')
                    {
                        sb.Append("\\t ");
                    }
                    else if (position.Value is '\n')
                    {
                        sb.Append("\\n ");
                    }
                    else if (position.Value is '\r')
                    {
                        sb.Append("\\r ");
                    }
                    else
                    {
                        sb.Append(position.Value + " ");
                    }
                }
            }

            return sb.ToString();
        }

        public static Grid operator >>(Grid grid, int shiftAmount)
        {
            int length = grid.length;
            foreach (var position in grid.grid.Values)
            {
                char column = position.Column;
                char value = position.Value;

                // Shift the column index to the right by the specified amount
                int shiftedColumnIndex = (grid.alphabetToUse.IndexOf(column) + shiftAmount) % length;
                char shiftedColumn = grid.alphabetToUse[shiftedColumnIndex];

                // Shift the value to the right by the specified amount
                int shiftedValueIndex = (grid.alphabetToUse.IndexOf(value) + shiftAmount) % length;
                char shiftedValue = grid.alphabetToUse[shiftedValueIndex];

                // Update the position in the grid
                position.Column = shiftedColumn;
                position.Value = shiftedValue;
            }
            return grid;
        }

        public static Grid operator <<(Grid grid, int shiftAmount)
        {
            int length = grid.length;
            foreach (var position in grid.grid.Values)
            {
                char column = position.Column;
                char value = position.Value;

                // Shift the column index to the left by the specified amount
                int shiftedColumnIndex = (grid.alphabetToUse.IndexOf(column) - shiftAmount + length) % length;
                char shiftedColumn = grid.alphabetToUse[shiftedColumnIndex];

                // Shift the value to the left by the specified amount
                int shiftedValueIndex = (grid.alphabetToUse.IndexOf(value) - shiftAmount + length) % length;
                char shiftedValue = grid.alphabetToUse[shiftedValueIndex];

                // Update the position in the grid
                position.Column = shiftedColumn;
                position.Value = shiftedValue;
            }
            return grid;
        }
    }
}
