using System.Buffers;
using System.Drawing;
using WinterRose.Recordium;

namespace WinterRose.Diff;

public class DiffEngine
{
    public const int WINDOW_SIZE = 2;
    public const ulong HASH_BASE = 257UL;

    class EditRegion
    {
        public long OldStart;
        public long OldEnd;
        public List<Op> Ops = new();
    }

    public abstract class Op
    {
        public long Offset { get; protected set; }
        protected Op(long offset) => Offset = offset;
        protected Op() { }
    }

    public class Delete : Op
    {
        public long Length { get; set; }
        private Delete() { } // serialization
        public Delete(long offset, long length) : base(offset) => Length = length;
    }

    public class Insert : Op
    {
        public byte[] Data { get; set; }
        private Insert() { } // serialization
        public Insert(long offset, byte[] data) : base(offset) => Data = data;
    }


    private class Match
    {
        public long OldStart;
        public long NewStart;
        public long Length;

        public long OldEnd => OldStart + Length;
        public long NewEnd => NewStart + Length;

        public bool IsMoveCandidate => OldStart != NewStart;
    }

    public List<Op> Diff(string oldPath, string newPath)
    {
        using FileView oldData = new FileView(oldPath);
        using FileView newData = new FileView(newPath);

        var ops = BuildOps(oldData, newData);
        ops = OptimizeOps(ops);
        return ops;
    }

    private List<Op> BuildOps(FileView oldData, FileView newData)
    {
        var ops = new List<Op>();

        long oldCursor = 0;
        long newCursor = 0;

        while (oldCursor < oldData.Length || newCursor < newData.Length)
        {

            // =========================
            // FAST PATH: ALIGNED
            // =========================
            if (oldCursor < oldData.Length &&
                newCursor < newData.Length &&
                oldData[oldCursor] == newData[newCursor])
            {
                oldCursor++;
                newCursor++;
                continue;
            }

            // =========================
            // TRY RESYNC (INSERT SIDE)
            // =========================
            if (TryResync(oldData, newData, oldCursor, newCursor, out long insertAdvance, out long deleteAdvance))
            {
                if (insertAdvance > 0)
                {
                    ops.Add(ReadInsert(newData, oldCursor, newCursor, insertAdvance));
                    newCursor += insertAdvance;
                }

                if (deleteAdvance > 0)
                {
                    ops.Add(new Delete(oldCursor, deleteAdvance));
                    oldCursor += deleteAdvance;
                }

                continue;
            }

            // =========================
            // FALLBACK: SINGLE BYTE DIFF
            // =========================
            if (newCursor < newData.Length && (oldCursor >= oldData.Length || ShouldTreatAsInsert(oldData, newData, oldCursor, newCursor)))
            {
                long insertStart = newCursor;
                long anchor = oldCursor;

                while (newCursor < newData.Length)
                {
                    bool resynced = TryResync(oldData, newData, oldCursor, newCursor, out _, out long testDelete);

                    if (resynced && testDelete > 0)
                        break;

                    bool solidGround = true;
                    for (int i = 0; i < 4; i++)
                    {
                        if (oldCursor + i >= oldData.Length || newCursor + i >= newData.Length ||
                            oldData[oldCursor + i] != newData[newCursor + i])
                        {
                            solidGround = false;
                            break;
                        }
                    }
                    if (solidGround) break;

                    newCursor++;
                }

                if (newCursor > insertStart)
                    ops.Add(ReadInsert(newData, anchor, insertStart, newCursor - insertStart));
            }
            else if (oldCursor < oldData.Length)
            {
                ops.Add(new Delete(oldCursor, 1));
                oldCursor++;
            }
        }

        return ops;
    }

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

    private List<Op> AdjustOffsetsForDelta(List<Op> ops)
    {
        // Sort by offset so we process ops in file order
        var sorted = ops.OrderBy(o => o.Offset).ToList();

        long delta = 0;
        var result = new List<Op>(sorted.Count);

        foreach (var op in sorted)
        {
            switch (op)
            {
                case Delete del:
                    // Delete offset stays anchored to the original file position,
                    // but we shift it by the accumulated delta from prior ops.
                    result.Add(new Delete(del.Offset + delta, del.Length));
                    delta -= del.Length; // deletes shrink the file
                    break;

                case Insert ins:
                    // Insert offset is also shifted by accumulated delta.
                    result.Add(new Insert(ins.Offset + delta, ins.Data));
                    delta += ins.Data.Length; // inserts grow the file
                    break;
            }
        }

        return result;
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

        // future:
        // ops = ConvertInsertDeleteToUpdate(ops);

        //ops = AdjustOffsetsForDelta(ops);
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
}