namespace WinterRose.Diff;

public class DiffEngine
{
    public const int WINDOW_SIZE = 2;
    public const ulong HASH_BASE = 257UL;

    public FileDiff Diff(string oldPath, string newPath)
    {
        using FileView oldData = new FileView(oldPath);
        using FileView newData = new FileView(newPath);

        var ops = BuildOps(oldData, newData);
        ops = OptimizeOps(ops);

        return new FileDiff(ops) 
        {
            NewFileHash = newData.ComputeSha256()
        };
    }

    // Tuning constants
    private const int CDC_MIN_CHUNK = 64;
    private const int CDC_TARGET_CHUNK = 512;
    private const int CDC_MAX_CHUNK = 4096;
    private const int CDC_WINDOW = 48;
    private const ulong CDC_MASK = (1UL << 9) - 1; // triggers ~every 512 bytes on average

    private List<Op> BuildOps(FileView oldData, FileView newData)
    {
        // 1. Find content-defined chunk boundaries in both files
        var oldBoundaries = FindChunkBoundaries(oldData);
        var newBoundaries = FindChunkBoundaries(newData);

        // 2. Hash every old chunk → offset lookup
        var oldChunkIndex = BuildChunkIndex(oldData, oldBoundaries);

        // 3. Walk new chunks, anchor on matches, emit ops for gaps
        var ops = new List<Op>();

        long oldCursor = 0;
        long newCursor = 0;

        for (int ni = 0; ni < newBoundaries.Count - 1; ni++)
        {
            long newChunkStart = newBoundaries[ni];
            long newChunkEnd = newBoundaries[ni + 1];
            long newChunkLen = newChunkEnd - newChunkStart;

            ulong hash = HashChunk(newData, newChunkStart, newChunkLen);

            if (oldChunkIndex.TryGetValue(hash, out long oldMatchStart))
            {
                // Verify it's not a hash collision
                if (ChunksEqual(oldData, oldMatchStart, newData, newChunkStart, newChunkLen))
                {
                    // Emit ops for the gap before this match
                    EmitGapOps(oldData, newData, oldCursor, oldMatchStart, newCursor, newChunkStart, ops);

                    // Advance both cursors past the matched region
                    oldCursor = oldMatchStart + newChunkLen;
                    newCursor = newChunkEnd;
                    continue;
                }
            }

            // No match found for this chunk — will be handled as a gap
        }

        // Emit ops for any remaining tail
        EmitGapOps(oldData, newData, oldCursor, oldData.Length, newCursor, newData.Length, ops);

        return ops;
    }

    private List<long> FindChunkBoundaries(FileView data)
    {
        var boundaries = new List<long> { 0 };

        if (data.Length < CDC_MIN_CHUNK)
        {
            boundaries.Add(data.Length);
            return boundaries;
        }

        // Buzhash rolling hash over CDC_WINDOW bytes
        ulong hash = 0;
        ulong[] buzhashTable = GetBuzhashTable();

        // Prime the window
        long i = 0;
        for (; i < CDC_WINDOW && i < data.Length; i++)
            hash ^= RotateLeft(buzhashTable[data[i]], (int)(CDC_WINDOW - 1 - (i % CDC_WINDOW)));

        long lastBoundary = 0;

        for (; i < data.Length; i++)
        {
            // Roll out the oldest byte, roll in the new one
            byte outgoing = data[i - CDC_WINDOW];
            byte incoming = data[i];
            hash = RotateLeft(hash, 1) ^ RotateLeft(buzhashTable[outgoing], CDC_WINDOW) ^ buzhashTable[incoming];

            long chunkLen = i - lastBoundary;

            if (chunkLen < CDC_MIN_CHUNK)
                continue;

            if ((hash & CDC_MASK) == 0 || chunkLen >= CDC_MAX_CHUNK)
            {
                boundaries.Add(i);
                lastBoundary = i;
            }
        }

        if (boundaries[^1] != data.Length)
            boundaries.Add(data.Length);

        return boundaries;
    }

    private Dictionary<ulong, long> BuildChunkIndex(FileView data, List<long> boundaries)
    {
        var index = new Dictionary<ulong, long>(boundaries.Count);

        for (int i = 0; i < boundaries.Count - 1; i++)
        {
            long start = boundaries[i];
            long len = boundaries[i + 1] - start;
            ulong hash = HashChunk(data, start, len);

            // First occurrence wins — keeps matches as early as possible
            index.TryAdd(hash, start);
        }

        return index;
    }

    private ulong HashChunk(FileView data, long start, long length)
    {
        // FNV-1a 64-bit — fast and good distribution for binary data
        ulong hash = 14695981039346656037UL;
        for (long i = 0; i < length; i++)
        {
            hash ^= data[start + i];
            hash *= 1099511628211UL;
        }
        return hash;
    }

    private bool ChunksEqual(FileView a, long aStart, FileView b, long bStart, long length)
    {
        for (long i = 0; i < length; i++)
            if (a[aStart + i] != b[bStart + i])
                return false;
        return true;
    }

    private void EmitGapOps(
        FileView oldData, FileView newData,
        long oldStart, long oldEnd,
        long newStart, long newEnd,
        List<Op> ops)
    {
        long oldLen = oldEnd - oldStart;
        long newLen = newEnd - newStart;

        if (oldLen == 0 && newLen == 0)
            return;

        if (oldLen == 0)
        {
            // Pure insert
            ops.Add(ReadInsert(newData, oldStart, newStart, newLen));
            return;
        }

        if (newLen == 0)
        {
            // Pure delete
            ops.Add(new Delete(oldStart, oldLen));
            return;
        }

        // Both sides have data — find the longest common prefix and suffix
        // to tighten the op to only the truly changed bytes
        long prefixLen = 0;
        while (prefixLen < oldLen && prefixLen < newLen &&
               oldData[oldStart + prefixLen] == newData[newStart + prefixLen])
            prefixLen++;

        long suffixLen = 0;
        while (suffixLen < oldLen - prefixLen && suffixLen < newLen - prefixLen &&
               oldData[oldEnd - 1 - suffixLen] == newData[newEnd - 1 - suffixLen])
            suffixLen++;

        long oldDiffStart = oldStart + prefixLen;
        long newDiffStart = newStart + prefixLen;
        long oldDiffLen = oldLen - prefixLen - suffixLen;
        long newDiffLen = newLen - prefixLen - suffixLen;

        if (oldDiffLen == 0 && newDiffLen == 0)
            return;

        if (oldDiffLen == 0)
        {
            ops.Add(ReadInsert(newData, oldDiffStart, newDiffStart, newDiffLen));
            return;
        }

        if (newDiffLen == 0)
        {
            ops.Add(new Delete(oldDiffStart, oldDiffLen));
            return;
        }

        // Changed region on both sides — emit as Delete + Insert
        // (ConvertInsertDeleteToUpdate will collapse these into an Update)
        ops.Add(new Delete(oldDiffStart, oldDiffLen));
        ops.Add(ReadInsert(newData, oldDiffStart, newDiffStart, newDiffLen));
    }

    // Buzhash table — one random 64-bit value per possible byte value
    private static ulong[]? _buzhashTable;
    private static readonly object _buzhashLock = new();

    private static ulong[] GetBuzhashTable()
    {
        if (_buzhashTable != null)
            return _buzhashTable;

        lock (_buzhashLock)
        {
            if (_buzhashTable != null)
                return _buzhashTable;

            var table = new ulong[256];
            var rng = new Random(0x57494E54); // fixed seed — must be deterministic
            byte[] buf = new byte[8];
            for (int i = 0; i < 256; i++)
            {
                rng.NextBytes(buf);
                table[i] = BitConverter.ToUInt64(buf, 0);
            }
            _buzhashTable = table;
            return table;
        }
    }

    private static ulong RotateLeft(ulong value, int count)
        => (value << count) | (value >> (64 - count));

    private bool TryResync(
        FileView oldData,
        FileView newData,
        long oldCursor,
        long newCursor,
        out long insertAdvance,
        out long deleteAdvance)
    {
        insertAdvance = 0;
        deleteAdvance = 0;

        const int LOOKAHEAD = 64;

        // =========================
        // TRY DELETE (old ahead)
        // =========================
        for (int i = 1; i <= LOOKAHEAD; i++)
        {
            if (oldCursor + i + WINDOW_SIZE > oldData.Length ||
                newCursor + WINDOW_SIZE > newData.Length)
                break;

            if (WindowMatch(oldData, newData, oldCursor + i, newCursor))
            {
                deleteAdvance = i;
                return true;
            }
        }

        // =========================
        // TRY INSERT (new ahead)
        // =========================
        for (int i = 1; i <= LOOKAHEAD; i++)
        {
            if (newCursor + i + WINDOW_SIZE >= newData.Length ||
                oldCursor + WINDOW_SIZE >= oldData.Length)
                break;

            if (WindowMatch(oldData, newData, oldCursor, newCursor + i))
            {
                insertAdvance = i;
                return true;
            }
        }

        return false;
    }

    private bool WindowMatch(FileView oldData, FileView newData, long oldStart, long newStart)
    {
        for (int i = 0; i < WINDOW_SIZE; i++)
        {
            if (oldStart + i >= oldData.Length || newStart + i >= newData.Length)
                return false;
            if (oldData[oldStart + i] != newData[newStart + i])
                return false;
        }
        return true;
    }

    private Insert ReadInsert(FileView newData, long oldAnchor, long newStart, long length)
    {
        byte[] data = new byte[length];
        for (long i = 0; i < length; i++)
            data[i] = newData[newStart + i];

        return new Insert(oldAnchor, data);
    }

    private bool ShouldTreatAsInsert(FileView oldData, FileView newData, long oldCursor, long newCursor)
    {
        if (newCursor >= newData.Length) return false;
        if (oldCursor >= oldData.Length) return true;

        const int SCAN = 8;
        byte nb = newData[newCursor];
        byte ob = oldData[oldCursor];

        for (int i = 1; i < SCAN && oldCursor + i < oldData.Length; i++)
            if (oldData[oldCursor + i] == nb) return true;  // new byte found ahead in old > insert

        for (int i = 1; i < SCAN && newCursor + i < newData.Length; i++)
            if (newData[newCursor + i] == ob) return false; // old byte found ahead in new > delete

        return true; // default to insert
    }

    private List<Op> OptimizeOps(List<Op> ops)
    {
        int oldLength;
        do
        {
            oldLength = ops.Count;
            ops = MergeInserts(ops);
        }
        while (oldLength != ops.Count);

        do
        {
            oldLength = ops.Count;
            ops = MergeDeletes(ops);
        }
        while (oldLength != ops.Count);

        ops = ConvertInsertDeleteToUpdate(ops);

        return ops;
    }

    private List<Op> MergeInserts(List<Op> ops)
    {
        var result = new List<Op>();
        Insert? current = null;

        foreach (var op in ops)
        {
            if (op is Insert ins)
            {
                if (current == null)
                {
                    current = ins;
                    continue;
                }

                if (current.Offset == ins.Offset || current.Offset + current.Data.Length == ins.Offset)
                {
                    byte[] merged = new byte[current.Data.Length + ins.Data.Length];
                    Array.Copy(current.Data, 0, merged, 0, current.Data.Length);
                    Array.Copy(ins.Data, 0, merged, current.Data.Length, ins.Data.Length);
                    current = new Insert(current.Offset, merged);
                }
                else
                {
                    result.Add(current);
                    current = ins;
                }
            }
            else
            {
                if (current != null)
                {
                    result.Add(current);
                    current = null;
                }
                result.Add(op);
            }
        }

        if (current != null)
            result.Add(current);

        return result;
    }

    private List<Op> MergeDeletes(List<Op> ops)
    {
        var result = new List<Op>();

        Delete? current = null;

        foreach (var op in ops)
        {
            if (op is Delete del)
            {
                if (current == null)
                {
                    current = del;
                    continue;
                }

                if (current.Offset + current.Length == del.Offset)
                {
                    current = new Delete(current.Offset, current.Length + del.Length);
                }
                else
                {
                    result.Add(current);
                    current = del;
                }
            }
            else
            {
                if (current != null)
                {
                    result.Add(current);
                    current = null;
                }

                result.Add(op);
            }
        }

        if (current != null)
            result.Add(current);

        return result;
    }

    private List<Op> ConvertInsertDeleteToUpdate(List<Op> ops)
    {
        var result = new List<Op>();

        for (int i = 0; i < ops.Count; i++)
        {
            // Case 1: Delete then Insert at same offset
            if (ops[i] is Delete del &&
                i + 1 < ops.Count &&
                ops[i + 1] is Insert ins &&
                ins.Offset == del.Offset)
            {
                result.Add(new Update(del.Offset, del.Length, ins.Data));
                i++;
                continue;
            }

            // Case 2: Insert then Delete at same offset (reversed order)
            if (ops[i] is Insert ins2 &&
                i + 1 < ops.Count &&
                ops[i + 1] is Delete del2 &&
                del2.Offset == ins2.Offset)
            {
                result.Add(new Update(ins2.Offset, del2.Length, ins2.Data));
                i++;
                continue;
            }

            // Case 3: Delete then Insert at delete's end offset (insert follows deleted region)
            if (ops[i] is Delete del3 &&
                i + 1 < ops.Count &&
                ops[i + 1] is Insert ins3 &&
                ins3.Offset == del3.Offset + del3.Length)
            {
                result.Add(new Update(del3.Offset, del3.Length, ins3.Data));
                i++;
                continue;
            }

            result.Add(ops[i]);
        }

        return result;
    }
}