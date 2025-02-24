using System;
using System.Collections.Generic;
using System.Linq;

namespace WinterRose;


public class Array256<T>
{
    private const int ChunkSize = 4;
    private Chunk[] chunks = [];
    int size;

    public Array256() { }

    public Array256(IEnumerable<T> data)
    {
        Int256 i = 0;
        var enumerator = data.GetEnumerator();
        enumerator.Reset();
        while (enumerator.MoveNext())
            Add(++i, enumerator.Current);
    }

    public static implicit operator Array256<T>(List<T> data) => new(data);

    public T this[Int256 index]
    {
        get
        {
            var (chunkIndex, innerIndex) = GetChunkIndexAndInnerIndex(index);

            if (chunkIndex >= chunks.Length)
            {
                throw new IndexOutOfRangeException("Index exceeds the current bounds of the array.");
            }

            return chunks[chunkIndex].data[innerIndex];
        }
        set
        {
            var (chunkIndex, innerIndex) = GetChunkIndexAndInnerIndex(index);

            if (chunkIndex >= chunks.Length)
            {
                int newLength = chunkIndex + 1;
                Array.Resize(ref chunks, newLength);

                for (int i = chunks.Length - 1; i < newLength; i++)
                    chunks[i] = new Chunk();
            }

            if (chunks[chunkIndex] is null)
                chunks[chunkIndex] = new Chunk();

            chunks[chunkIndex].data[innerIndex] = value;
        }
    }

    public void Add(Int256 index, T value)
    {
        this[index] = value;
    }
    public void Add(T element)
    {
        Int256 index = FindFirstAvailableIndex();
        this[index] = element;
    }

    private Int256 FindFirstAvailableIndex()
    {
        for (int chunkIndex = 0; chunkIndex < chunks.Length; chunkIndex++)
        {
            var chunk = chunks[chunkIndex];
            for (int innerIndex = 0; innerIndex < ChunkSize; innerIndex++)
            {
                if (EqualityComparer<T>.Default.Equals(chunk.data[innerIndex], default(T)))
                {
                    return new Int256((ulong)(chunkIndex * ChunkSize + innerIndex), 0, 0, 0);
                }
            }
        }

        int newChunkIndex = chunks.Length;
        Array.Resize(ref chunks, newChunkIndex + 1);
        chunks[newChunkIndex] = new Chunk();
        return new Int256((ulong)(newChunkIndex * ChunkSize), 0, 0, 0);
    }


    public void Remove(Int256 index)
    {
        this[index] = default(T);
    }

    public bool ContainsKey(Int256 index)
    {
        var (chunkIndex, innerIndex) = GetChunkIndexAndInnerIndex(index);
        if (chunkIndex >= chunks.Length)
            return false;
        if (innerIndex >= chunks[chunkIndex].data.Length)
            return false;
        return !EqualityComparer<T>.Default.Equals(chunks[chunkIndex].data[innerIndex], default);
    }

    public int Count => chunks.Sum(chunk => chunk.data.Count(e => !EqualityComparer<T>.Default.Equals(e, default(T))));

    public void Clear() => chunks = new Chunk[0];

    private (int chunkIndex, int innerIndex) GetChunkIndexAndInnerIndex(Int256 index)
    {
        ulong indexValue = index.ToUInt64(); 
        int chunkIndex = (int)(indexValue / ChunkSize); 
        int innerIndex = (int)(indexValue % ChunkSize);
        return (chunkIndex, innerIndex);
    }

    private class Chunk
    {
        public T[] data;

        public Chunk()
        {
            data = new T[ChunkSize];
        }
    }
}


