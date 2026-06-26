//using System.Buffers;
//using WinterRose.Recordium;

//namespace WinterRose.Diff;

//public class DiffEngine
//{
//    public const int WINDOW_SIZE = 64;
//    public const ulong HASH_BASE = 257UL;

//    public abstract class Op
//    {
//        public long Offset { get; protected set; }
//        protected Op(long offset) => Offset = offset;
//        protected Op() { }
//    }

//    public class Delete : Op
//    {
//        public long Length { get; set; }
//        public Delete(long offset, long length) : base(offset) => Length = length;
//    }

//    public class Insert : Op
//    {
//        public byte[] Data { get; set; }
//        public Insert(long offset, byte[] data) : base(offset) => Data = data;
//    }


//    private class Match
//    {
//        public long OldStart;
//        public long NewStart;
//        public long Length;

//        public long OldEnd => OldStart + Length;
//        public long NewEnd => NewStart + Length;

//        public bool IsMoveCandidate => OldStart != NewStart;
//    }

//    public List<Op> Diff(string oldPath, string newPath)
//    {
//        using FileView oldData = new FileView(oldPath);
//        using FileView newData = new FileView(newPath);

//        var ops = BuildOps(oldData, newData);
//        ops = OptimizeOps(ops);
//        return ops;
//    }

//    private List<Op> BuildOps(FileView oldData, FileView newData)
//    {
//        var ops = new List<Op>();

//        long oldCursor = 0;
//        long newCursor = 0;

//        while (oldCursor < oldData.Length || newCursor < newData.Length)
//        {
//            // =========================
//            // FAST PATH: ALIGNED
//            // =========================
//            if (oldCursor < oldData.Length &&
//                newCursor < newData.Length &&
//                oldData[oldCursor] == newData[newCursor])
//            {
//                oldCursor++;
//                newCursor++;
//                continue;
//            }

//            // =========================
//            // TRY RESYNC (INSERT SIDE)
//            // =========================
//            if (TryResync(oldData, newData, oldCursor, newCursor, out long insertAdvance, out long deleteAdvance))
//            {
//                if (insertAdvance > 0)
//                {
//                    ops.Add(ReadInsert(newData, newCursor, insertAdvance));
//                    newCursor += insertAdvance;
//                }

//                if (deleteAdvance > 0)
//                {
//                    ops.Add(new Delete(oldCursor, deleteAdvance));
//                    oldCursor += deleteAdvance;
//                }

//                continue;
//            }

//            // =========================
//            // FALLBACK: SINGLE BYTE DIFF
//            // =========================
//            if (newCursor < newData.Length && (oldCursor >= oldData.Length || ShouldTreatAsInsert(oldData, newData, oldCursor, newCursor)))
//            {
//                ops.Add(new Insert(newCursor, new byte[] { newData[newCursor] }));
//                newCursor++;
//            }
//            else if (oldCursor < oldData.Length)
//            {
//                ops.Add(new Delete(oldCursor, 1));
//                oldCursor++;
//            }
//        }

//        return ops;
//    }

//    private bool TryResync(
//        FileView oldData,
//        FileView newData,
//        long oldCursor,
//        long newCursor,
//        out long insertAdvance,
//        out long deleteAdvance)
//    {
//        insertAdvance = 0;
//        deleteAdvance = 0;

//        const int LOOKAHEAD = 64;

//        // =========================
//        // TRY INSERT (new ahead)
//        // =========================
//        for (int i = 1; i <= LOOKAHEAD; i++)
//        {
//            if (newCursor + i + WINDOW_SIZE >= newData.Length ||
//                oldCursor + WINDOW_SIZE >= oldData.Length)
//                break;

//            if (WindowMatch(oldData, newData, oldCursor, newCursor + i))
//            {
//                insertAdvance = i;
//                return true;
//            }
//        }

//        // =========================
//        // TRY DELETE (old ahead)
//        // =========================
//        for (int i = 1; i <= LOOKAHEAD; i++)
//        {
//            if (oldCursor + i + WINDOW_SIZE >= oldData.Length ||
//                newCursor + WINDOW_SIZE >= newData.Length)
//                break;

//            if (WindowMatch(oldData, newData, oldCursor + i, newCursor))
//            {
//                deleteAdvance = i;
//                return true;
//            }
//        }

//        return false;
//    }

//    private bool WindowMatch(FileView oldData, FileView newData, long oldStart, long newStart)
//    {
//        for (int i = 0; i < WINDOW_SIZE; i++)
//        {
//            if (oldData[oldStart + i] != newData[newStart + i])
//                return false;
//        }

//        return true;
//    }

//    private Insert ReadInsert(FileView newData, long start, long length)
//    {
//        byte[] data = new byte[length];

//        for (long i = 0; i < length; i++)
//            data[i] = newData[start + i];

//        return new Insert(start, data);
//    }

//    private bool ShouldTreatAsInsert(FileView oldData, FileView newData, long oldCursor, long newCursor)
//    {
//        if (newCursor >= newData.Length)
//            return false;

//        if (oldCursor >= oldData.Length)
//            return true;

//        return true;
//    }

//    private List<Op> OptimizeOps(List<Op> ops)
//    {
//        ops = MergeInserts(ops);
//        ops = MergeDeletes(ops);

//        // future:
//        // ops = ConvertInsertDeleteToUpdate(ops);

//        return ops;
//    }

//    private List<Op> MergeInserts(List<Op> ops)
//    {
//        var result = new List<Op>();

//        Insert? current = null;

//        foreach (var op in ops)
//        {
//            if (op is Insert ins)
//            {
//                if (current == null)
//                {
//                    current = ins;
//                    continue;
//                }

//                if (current.Offset + current.Data.Length == ins.Offset)
//                {
//                    // extend buffer
//                    byte[] merged = new byte[current.Data.Length + ins.Data.Length];

//                    Array.Copy(current.Data, 0, merged, 0, current.Data.Length);
//                    Array.Copy(ins.Data, 0, merged, current.Data.Length, ins.Data.Length);

//                    current = new Insert(current.Offset, merged);
//                }
//                else
//                {
//                    result.Add(current);
//                    current = ins;
//                }
//            }
//            else
//            {
//                if (current != null)
//                {
//                    result.Add(current);
//                    current = null;
//                }

//                result.Add(op);
//            }
//        }

//        if (current != null)
//            result.Add(current);

//        return result;
//    }

//    private List<Op> MergeDeletes(List<Op> ops)
//    {
//        var result = new List<Op>();

//        Delete? current = null;

//        foreach (var op in ops)
//        {
//            if (op is Delete del)
//            {
//                if (current == null)
//                {
//                    current = del;
//                    continue;
//                }

//                if (current.Offset + current.Length == del.Offset)
//                {
//                    current = new Delete(current.Offset, current.Length + del.Length);
//                }
//                else
//                {
//                    result.Add(current);
//                    current = del;
//                }
//            }
//            else
//            {
//                if (current != null)
//                {
//                    result.Add(current);
//                    current = null;
//                }

//                result.Add(op);
//            }
//        }

//        if (current != null)
//            result.Add(current);

//        return result;
//    }
//}