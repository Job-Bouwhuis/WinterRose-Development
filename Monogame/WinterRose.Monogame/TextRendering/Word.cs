using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections;
using System.Collections.Generic;

namespace WinterRose.Monogame.TextRendering;

public class Word : ICollection<Letter>
{
    private List<Letter> letters = [];
    public SpriteFont Font { get; set; } = MonoUtils.DefaultFont;

    public Letter this[int index]
    {
        get => letters[index];
        set => letters[index] = value;
    }

    public int Count => letters.Count;

    public bool IsReadOnly => false;

    public void Add(Letter item) => letters.Add(item);
    public void Clear() => letters.Clear();
    public bool Contains(Letter item) => letters.Contains(item);
    public void CopyTo(Letter[] array, int arrayIndex) => letters.CopyTo(array, arrayIndex);
    public bool Remove(Letter item) => letters.Remove(item);

    public IEnumerator<Letter> GetEnumerator() => letters.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => letters.GetEnumerator();

    public void SetAllColor(Color color)
    {
        foreach (var letter in letters)
            letter.Color = color;
    }

    public void EnableRandomLetterColor() => letters.ForEach(l => l.EnableRandomColors());
}
