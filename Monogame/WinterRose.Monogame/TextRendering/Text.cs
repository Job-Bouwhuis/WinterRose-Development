using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Xna.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using WinterRose.CrystalScripting.Legacy.Interpreting.Exceptions;

namespace WinterRose.Monogame.TextRendering;

public class Text : ICollection<Word>
{
    private readonly List<Word> words = [];

    public Text()
    {

    }

    public Text(string text)
    {
        ParseString(text);
    }

    private void ParseString(string s)
    {
        words.Clear();
        var currentWord = new Word();

        foreach (char c in s)
        {
            if (char.IsWhiteSpace(c))
            {
                if (currentWord.Count > 0)
                {
                    words.Add(currentWord);
                    currentWord = new Word();
                }
                if (c == '\n' || c == '\t' || c == '\r')
                {
                    Word separatorWord = [new Letter { Character = c }];
                    words.Add(separatorWord);
                }
            }
            else
            {
                currentWord.Add(new Letter { Character = c });
            }
        }

        if (currentWord.Count > 0)
        {
            words.Add(currentWord);
        }
    }

    public override int GetHashCode()
    {
        return words.GetHashCode();
    }

    public Word this[int index]
    {
        get => words[index];
        set => words[index] = value;
    }

    public int Count => words.Count;
    public bool IsReadOnly => false;

    public void SetText(string text)
    {
        ParseString(text);
    }

    public RectangleF CalculateBounds(Vector2 pos)
    {
        string stringRep = ToString();
        var size = words[0].Font.MeasureString(stringRep);
        return new(size.X, size.Y, pos.X, pos.Y);
    }

    public void EnableRandomLetterColor()
    {
        words.ForEach(word => word.EnableRandomLetterColor());
    }

    public void Add(Word item) => words.Add(item);
    public void Clear() => words.Clear();
    public bool Contains(Word item) => item != null && words.Contains(item);
    public void CopyTo(Word[] array, int arrayIndex) => words.CopyTo(array, arrayIndex);
    public bool Remove(Word item) => item != null && words.Remove(item);
    public IEnumerator<Word> GetEnumerator() => words.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => words.GetEnumerator();

    public static implicit operator Text(string s) => new Text(s);

    public override string ToString()
    {
        StringBuilder result = new StringBuilder();

        foreach (var word in words)
        {
            foreach (var letter in word)
                result.Append(letter.Character);
            result.Append(' ');
        }

        return result.ToString().Trim();
    }
}

