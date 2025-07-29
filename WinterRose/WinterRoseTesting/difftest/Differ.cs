using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnowLibraryTesting.difftest;

public struct Anchor
{
    public int originalOffset;
    public int modifiedOffset;
    public int length;

    public Anchor(int originalOffset, int modifiedOffset, int length)
    {
        this.originalOffset = originalOffset;
        this.modifiedOffset = modifiedOffset;
        this.length = length;
    }
}

public enum DiffOpType
{
    Insert,
    Delete,
    Update
}

[DebuggerDisplay("ToString()")]
public struct DiffOp
{
    public DiffOpType type;
    public long offset;  // Changed to long for big file support
    public byte[]? data;
    public int count;

    public DiffOp(DiffOpType type, long offset, int count, byte[]? data = null)
    {
        this.type = type;
        this.offset = offset;
        this.count = count;
        this.data = data;
    }

    public override readonly string ToString() => type switch
    {
        DiffOpType.Insert => $"INSERT @ {offset}, Bytes: [{string.Join(", ", data ?? [])}]",
        DiffOpType.Delete => $"DELETE @ {offset}, Count: {count}",
        DiffOpType.Update => $"UPDATE @ {offset}, Bytes: [{string.Join(", ", data ?? [])}]",
        _ => "Unknown"
    };
}


public class Differ
{
    public static List<List<DiffOp>> DiffFiles(FileInfo originalFile, FileInfo modifiedFile, long ChunkSizeBytes = 1L << 30)
    {
        FileStream original = originalFile.OpenRead();
        FileStream modified = modifiedFile.OpenRead();

        List<List<DiffOp>> result = [];

        while(original.Position != original.Length)
        {
            //byte[] orig = original.
        }

        throw new NotImplementedException();
    }


    public static byte[] ApplyDiffOps(byte[] original, List<DiffOp> ops)
    {
        List<byte> result = new();
        int readCursor = 0;

        foreach (var op in ops)
        {
            // 1. Copy unchanged region
            while (readCursor < op.offset)
            {
                result.Add(original[readCursor]);
                readCursor++;
            }

            // 2. Apply the operation
            switch (op.type)
            {
                case DiffOpType.Delete:
                    readCursor += op.count; // Skip deleted bytes
                    break;

                case DiffOpType.Insert:
                    if (op.data != null)
                        result.AddRange(op.data);
                    break;

                case DiffOpType.Update:
                    readCursor += op.count; // Skip old bytes
                    if (op.data != null)
                        result.AddRange(op.data); // Add new bytes
                    break;
            }
        }

        // 3. Copy remaining tail if any
        while (readCursor < original.Length)
        {
            result.Add(original[readCursor]);
            readCursor++;
        }

        return result.ToArray();
    }

    public static List<DiffOp> PostProcessDiffOps(List<DiffOp> ops)
    {
        List<DiffOp> optimized = new();
        int i = 0;

        while (i < ops.Count)
        {
            if (i + 1 < ops.Count &&
                ops[i].type == DiffOpType.Delete &&
                ops[i + 1].type == DiffOpType.Insert &&
                ops[i].offset == ops[i + 1].offset &&
                ops[i].count == ops[i + 1].count)
            {
                optimized.Add(new DiffOp(DiffOpType.Update, ops[i].offset, ops[i].count, ops[i + 1].data));
                i += 2;
            }
            else
            {
                optimized.Add(ops[i]);
                i++;
            }
        }

        return optimized;
    }


    public static List<DiffOp> BetweenAnchors(byte[] original, byte[] modified, List<Anchor> anchors)
    {
        var ops = new List<DiffOp>();

        for (int i = 0; i <= anchors.Count; i++)
        {
            // Define boundaries of diff region between anchors
            int origStart = i == 0 ? 0 : (int)(anchors[i - 1].originalOffset + anchors[i - 1].length);
            int origEnd = i == anchors.Count ? original.Length : (int)anchors[i].originalOffset;

            int modStart = i == 0 ? 0 : (int)(anchors[i - 1].modifiedOffset + anchors[i - 1].length);
            int modEnd = i == anchors.Count ? modified.Length : (int)anchors[i].modifiedOffset;

            // Extract slices
            byte[] origSlice = original[origStart..origEnd];
            byte[] modSlice = modified[modStart..modEnd];

            // If slices differ, recursively diff or just do naive byte compare here
            if (origSlice.Length > 0 || modSlice.Length > 0)
            {
                var subOps = FineGrainedDiff(origSlice, modSlice, origStart);
                ops.AddRange(subOps);
            }
        }

        return ops;
    }

    private static List<DiffOp> FineGrainedDiff(byte[] origSlice, byte[] modSlice, int globalOffset)
    {
        var ops = new List<DiffOp>();

        int origLen = origSlice.Length;
        int modLen = modSlice.Length;

        // 1. Find longest common prefix
        int prefixLength = 0;
        while (prefixLength < origLen && prefixLength < modLen && origSlice[prefixLength] == modSlice[prefixLength])
            prefixLength++;

        // 2. Find longest common suffix (from end, ignoring prefix area)
        int suffixLength = 0;
        while (suffixLength < (origLen - prefixLength) && suffixLength < (modLen - prefixLength) &&
               origSlice[origLen - 1 - suffixLength] == modSlice[modLen - 1 - suffixLength])
        {
            suffixLength++;
        }

        int origMiddleStart = prefixLength;
        int origMiddleEnd = origLen - suffixLength;
        int modMiddleStart = prefixLength;
        int modMiddleEnd = modLen - suffixLength;

        int origMiddleLength = origMiddleEnd - origMiddleStart;
        int modMiddleLength = modMiddleEnd - modMiddleStart;

        // If no middle region changed, just return empty ops
        if (origMiddleLength == 0 && modMiddleLength == 0)
            return ops;

        // 3. If middle region is small, do byte-wise compare and generate update/insert/delete
        if (origMiddleLength < 32 && modMiddleLength < 32)
        {
            int minLen = Math.Min(origMiddleLength, modMiddleLength);
            int i = 0;

            // Updates for overlapping bytes
            while (i < minLen)
            {
                if (origSlice[origMiddleStart + i] != modSlice[modMiddleStart + i])
                {
                    ops.Add(new DiffOp(
                        DiffOpType.Update,
                        globalOffset + origMiddleStart + i,
                        1,
                        new byte[] { modSlice[modMiddleStart + i] }
                    ));
                }
                i++;
            }

            // Deletes if original longer
            if (origMiddleLength > minLen)
            {
                ops.Add(new DiffOp(
                    DiffOpType.Delete,
                    globalOffset + origMiddleStart + minLen,
                    origMiddleLength - minLen
                ));
            }
            // Inserts if modified longer
            else if (modMiddleLength > minLen)
            {
                byte[] insertedBytes = new byte[modMiddleLength - minLen];
                Array.Copy(modSlice, modMiddleStart + minLen, insertedBytes, 0, insertedBytes.Length);
                ops.Add(new DiffOp(
                    DiffOpType.Insert,
                    globalOffset + origMiddleStart + minLen,
                    insertedBytes.Length,
                    insertedBytes
                ));
            }
            return ops;
        }

        // 4. For larger middle region, recursively diff

        // Optional: You can do a full anchor-based diff here or just split in half and recurse for simplicity

        // Simple recursive divide-and-conquer:
        int origMidHalf = origMiddleLength / 2;
        int modMidHalf = modMiddleLength / 2;

        // Left halves
        var leftOps = FineGrainedDiff(
            origSlice[origMiddleStart..(origMiddleStart + origMidHalf)],
            modSlice[modMiddleStart..(modMiddleStart + modMidHalf)],
            globalOffset + origMiddleStart);

        // Right halves
        var rightOps = FineGrainedDiff(
            origSlice[(origMiddleStart + origMidHalf)..origMiddleEnd],
            modSlice[(modMiddleStart + modMidHalf)..modMiddleEnd],
            globalOffset + origMiddleStart + origMidHalf);

        ops.AddRange(leftOps);
        ops.AddRange(rightOps);

        return ops;
    }


    public static List<Anchor> FindAnchors(byte[] original, byte[] modified, int windowSize = 12)
    {
        Dictionary<ulong, List<int>> originalHashes = new();
        List<Anchor> anchors = new();

        // 1. Index original file hashes
        for (int i = 0; i <= original.Length - windowSize; i++)
        {
            ulong hash = HashBytes(original, i, windowSize);
            if (!originalHashes.TryGetValue(hash, out var list))
            {
                list = new List<int>();
                originalHashes[hash] = list;
            }
            list.Add(i);
        }

        // 2. Search in modified file
        for (int j = 0; j <= modified.Length - windowSize; j++)
        {
            ulong hash = HashBytes(modified, j, windowSize);
            if (originalHashes.TryGetValue(hash, out var candidates))
            {
                foreach (int i in candidates)
                {
                    if (IsMatch(original, i, modified, j, windowSize))
                    {
                        anchors.Add(new Anchor(i, j, windowSize));
                        break; // only take first match to avoid duplication
                    }
                }
            }
        }

        // 3. Post-process to remove overlaps and sort
        return CleanupAnchors(anchors);
    }

    private static ulong HashBytes(byte[] data, int offset, int length)
    {
        ulong hash = 14695981039346656037UL;
        const ulong prime = 1099511628211UL;

        for (int i = 0; i < length; i++)
        {
            hash ^= data[offset + i];
            hash *= prime;
        }

        return hash;
    }

    private static bool IsMatch(byte[] a, int offsetA, byte[] b, int offsetB, int length)
    {
        for (int i = 0; i < length; i++)
            if (a[offsetA + i] != b[offsetB + i])
                return false;
        return true;
    }

    private static List<Anchor> CleanupAnchors(List<Anchor> anchors)
    {
        anchors.Sort((a, b) => a.originalOffset.CompareTo(b.originalOffset));

        List<Anchor> result = new();
        int lastOriginalEnd = -1;
        int lastModifiedEnd = -1;

        foreach (var anchor in anchors)
        {
            if (anchor.originalOffset >= lastOriginalEnd && anchor.modifiedOffset >= lastModifiedEnd)
            {
                result.Add(anchor);
                lastOriginalEnd = anchor.originalOffset + anchor.length;
                lastModifiedEnd = anchor.modifiedOffset + anchor.length;
            }
        }

        return result;
    }

}
