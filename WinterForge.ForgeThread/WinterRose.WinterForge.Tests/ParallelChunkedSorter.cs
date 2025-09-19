using System.Collections.Concurrent;

namespace WinterRose.ForgeThread.Tests;

public sealed class MultiThreadedSortedIterator<T> : IDisposable where T : IComparable<T>
{
    private readonly List<ConcurrentQueue<BufferEntry>> buffers;
    private readonly bool[] finished;
    private readonly T[] source;
    private readonly PooledThreadLoom loom;
    private volatile bool closed;

    private struct BufferEntry : IComparable<BufferEntry>
    {
        public readonly T value;
        public readonly int bufferIndex;
        public BufferEntry(T value, int bufferIndex)
        {
            this.value = value;
            this.bufferIndex = bufferIndex;
        }

        public int CompareTo(BufferEntry other) => value.CompareTo(other.value);
    }

    public MultiThreadedSortedIterator(PooledThreadLoom loom, T[] source, int threadCount)
    {
        this.loom = loom;
        this.source = source;
        this.buffers = Enumerable.Range(0, threadCount).Select(_ => new ConcurrentQueue<BufferEntry>()).ToList();
        this.finished = new bool[threadCount];

        int total = source.Length;
        int baseSize = total / threadCount;
        int remainder = total % threadCount;

        int cursor = 0;
        for (int i = 0; i < threadCount; i++)
        {
            int start = cursor;
            int length = baseSize + (i < remainder ? 1 : 0);
            int threadIndex = i;
            cursor += length;

            loom.Schedule(() => SortWorker(start, length, threadIndex));
        }
    }

    public bool HasNext()
    {
        return buffers.Any(b => !b.IsEmpty) || finished.Any(f => !f);
    }

    public T Next()
    {
        while (true)
        {
            var available = new List<BufferEntry>();

            for (int i = 0; i < buffers.Count; i++)
            {
                if (buffers[i].TryPeek(out var entry))
                    available.Add(entry);
            }

            if (available.Count > 0)
            {
                var min = available.Min();
                buffers[min.bufferIndex].TryDequeue(out _);
                return min.value;
            }

            if (finished.All(f => f))
                throw new InvalidOperationException("No more elements");

            Task.Yield();
        }
    }

    private void SortWorker(int start, int length, int threadIndex)
    {
        var slice = new ArraySegment<T>(source, start, length);
        var buffer = buffers[threadIndex];

        int sorted = 0;
        while (!closed && sorted < slice.Count)
        {
            for (int i = slice.Count - 1; i > sorted; i--)
            {
                var si = slice[i];
                var oi = slice[i + 1];
                if(si.CompareTo(oi) < 0)
                {
                    T temp = slice[i];
                    slice[i] = slice[i - 1];
                    slice[i - 1] = temp;
                }
            }

            T smallest = slice[sorted];
            sorted++;
            buffer.Enqueue(new BufferEntry(smallest, threadIndex));
        }

        finished[threadIndex] = true;
    }

    public void Dispose()
    {
        closed = true;
    }
}

