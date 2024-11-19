using System;
using System.Collections.Generic;
using System.Linq;

namespace WinterRose;


public class Array256<T>
{
    private const int ChunkSize = 1024;  // Size of each chunk (can be adjusted)
    private Chunk[] chunks = [];

    public Array256()
    {

    }

    public Array256(IEnumerable<T> data)
    {
        Int256 i = 0;
        var enumerator = data.GetEnumerator();
        enumerator.Reset();
        while (enumerator.MoveNext())
            Add(++i, enumerator.Current);
    }

    public static implicit operator Array256<T>(List<T> data) => new(data);

    // Access element at a specific index
    public T this[Int256 index]
    {
        get
        {
            var (chunkIndex, innerIndex) = GetChunkIndexAndInnerIndex(index);

            // Ensure chunk exists, if not, create it
            if (chunkIndex >= chunks.Length)
            {
                // Optionally, you can grow the chunks array or throw an exception if you want strict bounds.
                throw new IndexOutOfRangeException("Index exceeds the current bounds of the array.");
            }

            return chunks[chunkIndex].data[innerIndex];
        }
        set
        {
            var (chunkIndex, innerIndex) = GetChunkIndexAndInnerIndex(index);

            // Check if we need to grow the chunks array
            if (chunkIndex >= chunks.Length)
            {
                // Grow chunks array to fit the chunk index
                int newLength = chunkIndex + 1;
                Array.Resize(ref chunks, newLength);

                // Initialize new chunks as needed
                for (int i = chunks.Length - 1; i < newLength; i++)
                {
                    chunks[i] = new Chunk();
                }
            }

            // Assign the value to the appropriate chunk and index
            if (chunks[chunkIndex] is null)
                chunks[chunkIndex] = new Chunk();

            chunks[chunkIndex].data[innerIndex] = value;
        }
    }


    // Add an element at the specific index
    public void Add(Int256 index, T value)
    {
        this[index] = value;  // Uses the indexer
    }
    public void Add(T element)
    {
        // Find the first available index
        Int256 index = FindFirstAvailableIndex();
        this[index] = element;
    }

    // Method to find the first available index
    private Int256 FindFirstAvailableIndex()
    {
        // Loop through the chunks and check for the first available index
        for (int chunkIndex = 0; chunkIndex < chunks.Length; chunkIndex++)
        {
            var chunk = chunks[chunkIndex];
            for (int innerIndex = 0; innerIndex < ChunkSize; innerIndex++)
            {
                if (EqualityComparer<T>.Default.Equals(chunk.data[innerIndex], default(T)))
                {
                    return new Int256((ulong)(chunkIndex * ChunkSize + innerIndex), 0, 0, 0); // Generate index
                }
            }
        }

        // If no available index found, allocate a new chunk
        int newChunkIndex = chunks.Length;
        Array.Resize(ref chunks, newChunkIndex + 1);
        chunks[newChunkIndex] = new Chunk();
        return new Int256((ulong)(newChunkIndex * ChunkSize), 0, 0, 0); // Return the first index of the new chunk
    }


    // Remove element by its index (sets it to default(T))
    public void Remove(Int256 index)
    {
        this[index] = default(T);  // Set the value to default (null or 0 depending on type)
    }

    // Check if an index exists (is not default value)
    public bool ContainsKey(Int256 index)
    {
        var (chunkIndex, innerIndex) = GetChunkIndexAndInnerIndex(index);
        if (chunkIndex >= chunks.Length)
            return false;
        if (innerIndex >= chunks[chunkIndex].data.Length)
            return false;
        return !EqualityComparer<T>.Default.Equals(chunks[chunkIndex].data[innerIndex], default);
    }

    // Get total count of elements (non-default)
    public int Count => chunks.Sum(chunk => chunk.data.Count(e => !EqualityComparer<T>.Default.Equals(e, default(T))));

    // Clear all elements
    public void Clear()
    {
        chunks = new Chunk[0];  // Start fresh
    }

    // Helper method to get the chunk index and the index inside the chunk
    private (int chunkIndex, int innerIndex) GetChunkIndexAndInnerIndex(Int256 index)
    {
        ulong indexValue = index.ToUInt64();  // Convert to ulong for easier chunk mapping
        int chunkIndex = (int)(indexValue / ChunkSize);  // Which chunk does this index belong to
        int innerIndex = (int)(indexValue % ChunkSize);  // The exact index inside that chunk
        return (chunkIndex, innerIndex);
    }

    // Chunk data structure that holds an array of type T
    private class Chunk
    {
        public T[] data;

        public Chunk()
        {
            data = new T[ChunkSize];  // Initialize with a chunk size
        }
    }
}


