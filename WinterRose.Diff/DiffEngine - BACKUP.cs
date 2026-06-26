//namespace WinterRose.Diff;

//public class DiffEngine
//{
//    public const int WINDOW_SIZE = 2;
//    public const ulong HASH_BASE = 257UL;

//    public FileDiff Diff(string oldPath, string newPath)
//    {
//        using FileView oldData = new FileView(oldPath);
//        using FileView newData = new FileView(newPath);

//        var ops = BuildOps(oldData, newData);
//        ops = OptimizeOps(ops);

//        return new FileDiff(ops)
//        {
//            NewFileHash = newData.ComputeSha256()
//        };
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
//                    ops.Add(ReadInsert(newData, oldCursor, newCursor, insertAdvance));
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
//                long insertStart = newCursor;
//                long anchor = oldCursor;

//                while (newCursor < newData.Length)
//                {
//                    bool resynced = TryResync(oldData, newData, oldCursor, newCursor, out _, out long testDelete);

//                    if (resynced && testDelete > 0)
//                        break;

//                    bool solidGround = true;
//                    for (int i = 0; i < 4; i++)
//                    {
//                        if (oldCursor + i >= oldData.Length || newCursor + i >= newData.Length ||
//                            oldData[oldCursor + i] != newData[newCursor + i])
//                        {
//                            solidGround = false;
//                            break;
//                        }
//                    }
//                    if (solidGround) break;

//                    newCursor++;
//                }

//                if (newCursor > insertStart)
//                    ops.Add(ReadInsert(newData, anchor, insertStart, newCursor - insertStart));
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
//        // TRY DELETE (old ahead)
//        // =========================
//        for (int i = 1; i <= LOOKAHEAD; i++)
//        {
//            if (oldCursor + i + WINDOW_SIZE > oldData.Length ||
//                newCursor + WINDOW_SIZE > newData.Length)
//                break;

//            if (WindowMatch(oldData, newData, oldCursor + i, newCursor))
//            {
//                deleteAdvance = i;
//                return true;
//            }
//        }

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

//        return false;
//    }

//    private bool WindowMatch(FileView oldData, FileView newData, long oldStart, long newStart)
//    {
//        for (int i = 0; i < WINDOW_SIZE; i++)
//        {
//            if (oldStart + i >= oldData.Length || newStart + i >= newData.Length)
//                return false;
//            if (oldData[oldStart + i] != newData[newStart + i])
//                return false;
//        }
//        return true;
//    }

//    private Insert ReadInsert(FileView newData, long oldAnchor, long newStart, long length)
//    {
//        byte[] data = new byte[length];
//        for (long i = 0; i < length; i++)
//            data[i] = newData[newStart + i];

//        return new Insert(oldAnchor, data);
//    }

//    private bool ShouldTreatAsInsert(FileView oldData, FileView newData, long oldCursor, long newCursor)
//    {
//        if (newCursor >= newData.Length) return false;
//        if (oldCursor >= oldData.Length) return true;

//        const int SCAN = 8;
//        byte nb = newData[newCursor];
//        byte ob = oldData[oldCursor];

//        for (int i = 1; i < SCAN && oldCursor + i < oldData.Length; i++)
//            if (oldData[oldCursor + i] == nb) return true;  // new byte found ahead in old > insert

//        for (int i = 1; i < SCAN && newCursor + i < newData.Length; i++)
//            if (newData[newCursor + i] == ob) return false; // old byte found ahead in new > delete

//        return true; // default to insert
//    }

//    private List<Op> OptimizeOps(List<Op> ops)
//    {
//        int oldLength;
//        do
//        {
//            oldLength = ops.Count;
//            ops = MergeInserts(ops);
//        }
//        while (oldLength != ops.Count);

//        do
//        {
//            oldLength = ops.Count;
//            ops = MergeDeletes(ops);
//        }
//        while (oldLength != ops.Count);

//        ops = ConvertInsertDeleteToUpdate(ops);

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

//                if (current.Offset == ins.Offset || current.Offset + current.Data.Length == ins.Offset)
//                {
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

//    private List<Op> ConvertInsertDeleteToUpdate(List<Op> ops)
//    {
//        var result = new List<Op>();

//        for (int i = 0; i < ops.Count; i++)
//        {
//            // Case 1: Delete then Insert at same offset
//            if (ops[i] is Delete del &&
//                i + 1 < ops.Count &&
//                ops[i + 1] is Insert ins &&
//                ins.Offset == del.Offset)
//            {
//                result.Add(new Update(del.Offset, del.Length, ins.Data));
//                i++;
//                continue;
//            }

//            // Case 2: Insert then Delete at same offset (reversed order)
//            if (ops[i] is Insert ins2 &&
//                i + 1 < ops.Count &&
//                ops[i + 1] is Delete del2 &&
//                del2.Offset == ins2.Offset)
//            {
//                result.Add(new Update(ins2.Offset, del2.Length, ins2.Data));
//                i++;
//                continue;
//            }

//            // Case 3: Delete then Insert at delete's end offset (insert follows deleted region)
//            if (ops[i] is Delete del3 &&
//                i + 1 < ops.Count &&
//                ops[i + 1] is Insert ins3 &&
//                ins3.Offset == del3.Offset + del3.Length)
//            {
//                result.Add(new Update(del3.Offset, del3.Length, ins3.Data));
//                i++;
//                continue;
//            }

//            result.Add(ops[i]);
//        }

//        return result;
//    }
//}