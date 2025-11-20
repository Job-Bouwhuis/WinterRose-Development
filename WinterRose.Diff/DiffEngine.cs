using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.Recordium;

namespace WinterRose.Diff;

public class DiffEngine
{
    // Tunable defaults
    public static readonly int[] BLOCK_SIZES = new int[] { 1024, 256, 64 };
    public static readonly int[] STRIDES = new int[] { 64, 16, 4 }; // how far to advance window each step per pass
    public const ulong HASH_BASE = 257UL; // polynomial base

    public abstract class Op
    {
        [WFInclude]
        public long NewOffset { get; set; }
        protected Op(long newOffset) { NewOffset = newOffset; }
        protected Op() { } // for serialization
    }

    public class Copy : Op
    {
        [WFInclude]
        public long OldOffset { get; private set; }
        [WFInclude]
        public int Length { get; private set; }
        public Copy(long oldOffset, long newOffset, int length) : base(newOffset)
        {
            OldOffset = oldOffset;
            Length = length;
        }
        private Copy() { } // for serialization
        public override string ToString() => $"COPY old@{OldOffset} -> new@{NewOffset} len={Length}";
    }

    public class Insert : Op
    {
        [WFInclude]
        public byte[] Data { get; private set; }
        //[WFInclude]
        //private string _dataBase64
        //{
        //    get => Convert.ToBase64String(Data);
        //    set => Data = Convert.FromBase64String(value);
        //}
        public Insert(long newOffset, byte[] data) : base(newOffset) { Data = data; }
        private Insert() { } // for serialization
        public override string ToString() => $"INSERT new@{NewOffset} len={Data.Length}";
    }

    public class Delete : Op
    {
        [WFInclude]
        public long OldOffset { get; private set; }
        [WFInclude]
        public long Length { get; private set; }
        public Delete(long oldOffset, long length) : base(-1) { OldOffset = oldOffset; Length = length; }
        private Delete() { } // for serialization
        public override string ToString() => $"DELETE old@{OldOffset} len={Length}";
    }

    public class CreateFile : Op
    {
        [WFInclude]
        public byte[] Data { get; set; }
        [WFExclude]
        public string FullName { get; set; }

        public CreateFile(byte[] data) { Data = data; }
        private CreateFile() { } // for serialization
        public override string ToString() => $"CREATE FILE len={Data.Length}";
    }

    public class DeleteFile : Op
    {
        public DeleteFile() { }
        [WFExclude]
        public string FullName { get; set; }
        public override string ToString() => $"DELETE FILE";
    }

    public async Task<List<Op>> DiffAsync(PipeReader oldReader, PipeReader newReader)
    {
        string oldTemp = Path.GetTempFileName();
        string newTemp = Path.GetTempFileName();

        try
        {
            await CopyPipeToFileAsync(oldReader, oldTemp).ConfigureAwait(false);
            await CopyPipeToFileAsync(newReader, newTemp).ConfigureAwait(false);

            using (var oldFs = new FileStream(oldTemp, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var newFs = new FileStream(newTemp, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                long oldLen = oldFs.Length;
                long newLen = newFs.Length;

                // map of newOffset -> Match(oldOffset,length)
                var matches = new SortedDictionary<int, Match>();

                for (int pass = 0; pass < BLOCK_SIZES.Length; pass++)
                {
                    int blockSize = BLOCK_SIZES[pass];
                    int stride = STRIDES[pass];

                    if (oldLen < blockSize || newLen < blockSize) continue;

                    var index = BuildIndexFromFile(oldFs, blockSize);

                    for (int newPos = 0; newPos + blockSize <= newLen; newPos += stride)
                    {
                        ulong h = await HashBytesFromFileAsync(newFs, newPos, blockSize).ConfigureAwait(false);
                        if (!index.TryGetValue(h, out var candidates)) continue;

                        foreach (int oldPos in candidates)
                        {
                            if (IsOverlappingWithExisting(oldPos, blockSize, matches) || IsNewOverlapping(newPos, blockSize, matches))
                                continue;

                            if (await ExactMatchFileAsync(oldFs, oldPos, newFs, newPos, blockSize).ConfigureAwait(false))
                            {
                                int extendBack = await ExtendBackwardFileAsync(oldFs, newFs, oldPos, newPos).ConfigureAwait(false);
                                int oldStart = oldPos - extendBack;
                                int newStart = newPos - extendBack;

                                int extendForward = await ExtendForwardFileAsync(oldFs, newFs, oldPos, newPos, blockSize).ConfigureAwait(false);
                                int fullLen = extendBack + blockSize + extendForward;

                                if (!matches.ContainsKey(newStart))
                                {
                                    matches[newStart] = new Match(oldStart, fullLen);
                                }
                                else
                                {
                                    var existing = matches[newStart];
                                    if (fullLen > existing.Length)
                                        matches[newStart] = new Match(oldStart, fullLen);
                                }

                                // skip ahead in newPos to avoid re-detecting inside the same match
                                newPos = Math.Max(newPos, newStart + fullLen - stride);
                                break;
                            }
                        }
                    }
                }

                var ops = new List<Op>();
                long cursorNew = 0;
                var enumerator = matches.GetEnumerator();
                bool hasEntry = enumerator.MoveNext();
                while (cursorNew < newLen)
                {
                    if (hasEntry)
                    {
                        var entry = enumerator.Current;
                        int matchNewPos = entry.Key;
                        var m = entry.Value;
                        if (matchNewPos < cursorNew)
                        {
                            hasEntry = enumerator.MoveNext();
                            continue;
                        }
                        if (matchNewPos > cursorNew)
                        {
                            int insLen = matchNewPos - (int)cursorNew;
                            byte[] insData = ReadBytesFromFile(newFs, cursorNew, insLen);
                            ops.Add(new Insert(cursorNew, insData));
                            cursorNew = matchNewPos;
                        }
                        ops.Add(new Copy(m.OldOffset, matchNewPos, m.Length));
                        cursorNew += m.Length;
                        hasEntry = enumerator.MoveNext();
                    }
                    else
                    {
                        if (cursorNew < newLen)
                        {
                            int insLen = (int)(newLen - cursorNew);
                            byte[] insData = ReadBytesFromFile(newFs, cursorNew, insLen);
                            ops.Add(new Insert(cursorNew, insData));
                        }
                        break;
                    }
                }

                var deletes = ComputeDeletesFromCopies(ops, (int)oldLen);
                ops.AddRange(deletes);

                var merged = MergeAdjacentOps(ops);
                return merged;
            }
        }
        finally
        {
            TryDeleteTemp(oldTemp);
            TryDeleteTemp(newTemp);
        }
    }

    private static void TryDeleteTemp(string path)
    {
        try { if (File.Exists(path)) File.Delete(path); } catch { }
    }

    private static async Task CopyPipeToFileAsync(PipeReader reader, string path)
    {
        using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            while (true)
            {
                ReadResult result = await reader.ReadAsync().ConfigureAwait(false);
                ReadOnlySequence<byte> buffer = result.Buffer;
                foreach (var segment in buffer)
                {
                    await fs.WriteAsync(segment).ConfigureAwait(false);
                }
                reader.AdvanceTo(buffer.End);
                if (result.IsCompleted) break;
            }
            await fs.FlushAsync().ConfigureAwait(false);
        }
    }

    // Build index from file using non-overlapping anchors
    private Dictionary<ulong, List<int>> BuildIndexFromFile(FileStream fs, int blockSize)
    {
        var index = new Dictionary<ulong, List<int>>();
        long length = fs.Length;
        byte[] block = new byte[blockSize];
        for (long pos = 0; pos + blockSize <= length; pos += blockSize)
        {
            fs.Seek(pos, SeekOrigin.Begin);
            int read = fs.Read(block, 0, blockSize);
            if (read != blockSize) break;
            ulong h = HashBytes(block, 0, blockSize);
            if (!index.TryGetValue(h, out var list))
            {
                list = new List<int>();
                index[h] = list;
            }
            list.Add((int)pos);
        }
        return index;
    }

    private static async Task<ulong> HashBytesFromFileAsync(FileStream fs, long offset, int length)
    {
        byte[] buf = new byte[length];
        fs.Seek(offset, SeekOrigin.Begin);
        int read = await fs.ReadAsync(buf, 0, length).ConfigureAwait(false);
        if (read != length) throw new EndOfStreamException();
        return HashBytes(buf, 0, length);
    }

    private static ulong HashBytes(byte[] arr, int offset, int length)
    {
        unchecked
        {
            ulong h = 0;
            for (int i = 0; i < length; i++)
            {
                h = h * HASH_BASE + (ulong)(arr[offset + i] & 0xff);
            }
            return h;
        }
    }

    private async Task<bool> ExactMatchFileAsync(FileStream oldFs, int oldPos, FileStream newFs, int newPos, int len)
    {
        const int chunk = 8192;
        int remaining = len;
        byte[] a = new byte[Math.Min(chunk, len)];
        byte[] b = new byte[Math.Min(chunk, len)];
        int aOff = 0, bOff = 0;
        while (remaining > 0)
        {
            int toRead = Math.Min(remaining, chunk);
            oldFs.Seek(oldPos + (len - remaining), SeekOrigin.Begin);
            newFs.Seek(newPos + (len - remaining), SeekOrigin.Begin);
            int ra = await oldFs.ReadAsync(a, 0, toRead).ConfigureAwait(false);
            int rb = await newFs.ReadAsync(b, 0, toRead).ConfigureAwait(false);
            if (ra != rb) return false;
            for (int i = 0; i < ra; i++) if (a[i] != b[i]) return false;
            remaining -= toRead;
        }
        return true;
    }

    private async Task<int> ExtendBackwardFileAsync(FileStream oldFs, FileStream newFs, int oldPos, int newPos)
    {
        int maxBack = Math.Min(oldPos, newPos);
        int back = 0;
        while (back < maxBack)
        {
            // compare single bytes backwards
            oldFs.Seek(oldPos - back - 1, SeekOrigin.Begin);
            newFs.Seek(newPos - back - 1, SeekOrigin.Begin);
            int a = oldFs.ReadByte();
            int b = newFs.ReadByte();
            if (a == -1 || b == -1 || a != b) break;
            back++;
        }
        return back;
    }

    private async Task<int> ExtendForwardFileAsync(FileStream oldFs, FileStream newFs, int oldPos, int newPos, int baseMatchLen)
    {
        long offOld = oldPos + baseMatchLen;
        long offNew = newPos + baseMatchLen;
        int forward = 0;
        long oldLen = oldFs.Length;
        long newLen = newFs.Length;
        while (offOld + forward < oldLen && offNew + forward < newLen)
        {
            oldFs.Seek(offOld + forward, SeekOrigin.Begin);
            newFs.Seek(offNew + forward, SeekOrigin.Begin);
            int a = oldFs.ReadByte();
            int b = newFs.ReadByte();
            if (a == -1 || b == -1 || a != b) break;
            forward++;
        }
        return forward;
    }

    private static bool IsOverlappingWithExisting(int oldPos, int len, SortedDictionary<int, Match> matches)
    {
        foreach (var m in matches.Values)
        {
            if (RangesOverlap(oldPos, oldPos + len, m.OldOffset, m.OldOffset + m.Length)) return true;
        }
        return false;
    }

    private static bool IsNewOverlapping(int newPos, int len, SortedDictionary<int, Match> matches)
    {
        int? floor = GetFloorKey(matches, newPos);
        if (floor.HasValue)
        {
            var m = matches[floor.Value];
            if (m != null && floor.Value + m.Length > newPos) return true;
        }
        int? ceil = GetCeilingKey(matches, newPos);
        if (ceil.HasValue && ceil.Value < newPos + len) return true;
        return false;
    }

    private static int? GetFloorKey(SortedDictionary<int, Match> dict, int key)
    {
        foreach (var k in dict.Keys.Reverse()) if (k <= key) return k;
        return null;
    }

    private static int? GetCeilingKey(SortedDictionary<int, Match> dict, int key)
    {
        foreach (var k in dict.Keys) if (k >= key) return k;
        return null;
    }

    private static bool ExactMatch(byte[] a, int offA, byte[] b, int offB, int len)
    {
        if (offA < 0 || offB < 0 || offA + len > a.Length || offB + len > b.Length) return false;
        for (int i = 0; i < len; i++) if (a[offA + i] != b[offB + i]) return false;
        return true;
    }

    private static int ExtendBackward(byte[] oldBytes, byte[] newBytes, int oldPos, int newPos)
    {
        int maxBack = Math.Min(oldPos, newPos);
        int back = 0;
        while (back < maxBack && oldBytes[oldPos - back - 1] == newBytes[newPos - back - 1]) back++;
        return back;
    }

    private static int ExtendForward(byte[] oldBytes, byte[] newBytes, int oldPos, int newPos, int baseMatchLen)
    {
        int offOld = oldPos + baseMatchLen;
        int offNew = newPos + baseMatchLen;
        int forward = 0;
        while (offOld + forward < oldBytes.Length && offNew + forward < newBytes.Length && oldBytes[offOld + forward] == newBytes[offNew + forward])
        {
            forward++;
        }
        return forward;
    }

    private static bool RangesOverlap(int a1, int a2, int b1, int b2)
    {
        return a1 < b2 && b1 < a2;
    }

    private static byte[] ReadBytesFromFile(FileStream fs, long offset, int length)
    {
        byte[] buf = new byte[length];
        fs.Seek(offset, SeekOrigin.Begin);
        int read = fs.Read(buf, 0, length);
        if (read != length)
        {
            if (read < 0) return new byte[0];
            if (read != length)
            {
                var smaller = new byte[read];
                Array.Copy(buf, smaller, read);
                return smaller;
            }
        }
        return buf;
    }

    private List<Delete> ComputeDeletesFromCopies(List<Op> ops, int oldLen)
    {
        var ranges = new List<int[]>();
        foreach (var op in ops)
        {
            if (op is Copy c)
            {
                ranges.Add(new int[] { (int)c.OldOffset, (int)(c.OldOffset + c.Length) });
            }
        }
        ranges.Sort((a, b) => a[0].CompareTo(b[0]));
        var deletes = new List<Delete>();
        int cursor = 0;
        foreach (var r in ranges)
        {
            if (r[0] > cursor)
            {
                deletes.Add(new Delete(cursor, r[0] - cursor));
            }
            cursor = Math.Max(cursor, r[1]);
        }
        if (cursor < oldLen) deletes.Add(new Delete(cursor, oldLen - cursor));
        return deletes;
    }

    private List<Op> MergeAdjacentOps(List<Op> ops)
    {
        var outList = new List<Op>();

        // Ensure deterministic order for merging:
        // - Copies/Inserts ordered by their NewOffset
        // - Deletes (NewOffset == -1) placed after everything
        var ordered = ops.OrderBy(o =>
        {
            return o switch
            {
                Insert i => i.NewOffset,
                Copy c => c.NewOffset,
                _ => long.MaxValue,
            };
        });

        foreach (var op in ordered)
        {
            if (outList.Count > 0 && op is Insert curInsert && outList[^1] is Insert prevInsert)
            {
                // Only merge inserts that are contiguous in the new file
                if (prevInsert.NewOffset + prevInsert.Data.Length == curInsert.NewOffset)
                {
                    var merged = new byte[prevInsert.Data.Length + curInsert.Data.Length];
                    Buffer.BlockCopy(prevInsert.Data, 0, merged, 0, prevInsert.Data.Length);
                    Buffer.BlockCopy(curInsert.Data, 0, merged, prevInsert.Data.Length, curInsert.Data.Length);
                    outList[^1] = new Insert(prevInsert.NewOffset, merged);
                    continue;
                }
            }

            if (outList.Count > 0 && op is Copy curCopy && outList[^1] is Copy prevCopy)
            {
                if (prevCopy.OldOffset + prevCopy.Length == curCopy.OldOffset &&
                    prevCopy.NewOffset + prevCopy.Length == curCopy.NewOffset)
                {
                    outList[^1] = new Copy(prevCopy.OldOffset, prevCopy.NewOffset, prevCopy.Length + curCopy.Length);
                    continue;
                }
            }

            outList.Add(op);
        }

        return outList;
    }

    private class Match
    {
        public readonly int OldOffset;
        public readonly int Length;
        public Match(int oldOffset, int length) { OldOffset = oldOffset; Length = length; }
    }

    public async Task<DirectoryDiff> DirectoryDiffAsync(string oldDir, string newDir)
    {
        Log log = new Log("DirectoryDiff");
        var dirDiff = new DirectoryDiff();
        var oldFiles = Directory.GetFiles(oldDir, "*", SearchOption.AllDirectories)
            .Select(f => f[oldDir.Length..].TrimStart(Path.DirectorySeparatorChar))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var newFiles = Directory.GetFiles(newDir, "*", SearchOption.AllDirectories)
            .Select(f => f[newDir.Length..].TrimStart(Path.DirectorySeparatorChar))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var allFiles = new HashSet<string>(oldFiles, StringComparer.OrdinalIgnoreCase);
        allFiles.UnionWith(newFiles);
        foreach (var relativePath in allFiles)
        {
            log.Info(newDir + ": Processing file " + relativePath);
            string oldPath = Path.Combine(oldDir, relativePath);
            string newPath = Path.Combine(newDir, relativePath);
            bool oldExists = File.Exists(oldPath);
            bool newExists = File.Exists(newPath);
            if (oldExists && newExists)
            {
                using (var oldFs = new FileStream(oldPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (var newFs = new FileStream(newPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    var diffOps = await FileDiffAsync(oldFs, newFs);
                    dirDiff.FileDiffs[relativePath] = diffOps;
                }
            }
            else if (oldExists && !newExists)
            {
                dirDiff.FileDiffs[relativePath] = new FileDiff { ops = new List<Op> { new Delete(0, new FileInfo(oldPath).Length) } };
            }
            else if (!oldExists && newExists)
            {
                using (var newFs = new FileStream(newPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    byte[] newData = new byte[newFs.Length];
                    await newFs.ReadAsync(newData, 0, (int)newFs.Length);
                    dirDiff.FileDiffs[relativePath] = new FileDiff { ops = new List<Op> { new Insert(0, newData) } };
                }
            }
        }
        return dirDiff;
    }

    // quick synchronous test wrapper: convenience for testing with Streams
    public async Task<FileDiff> FileDiffAsync(Stream oldStream, Stream newStream, bool keepTempFilesForDebug = false)
    {
        // List<Op>
        // List<ChangeSequenceBuilder.Change>

        var oldPipe = new Pipe();
        var newPipe = new Pipe();

        // pump oldStream -> oldPipe
        _ = Task.Run(async () =>
        {
            // rewind if possible to ensure full content
            TrySeekToStart(oldStream);
            await oldStream.CopyToAsync(oldPipe.Writer).ConfigureAwait(false);
            await oldPipe.Writer.CompleteAsync().ConfigureAwait(false);
        });

        // pump newStream -> newPipe
        _ = Task.Run(async () =>
        {
            TrySeekToStart(newStream);
            await newStream.CopyToAsync(newPipe.Writer).ConfigureAwait(false);
            await newPipe.Writer.CompleteAsync().ConfigureAwait(false);
        });

        var (ops, oldLen, newLen, oldTemp, newTemp) = await DiffWithDiagnosticsAsync(oldPipe.Reader, newPipe.Reader, keepTempFilesForDebug).ConfigureAwait(false);

        return ops;
    }

    private static void TrySeekToStart(Stream s)
    {
        try
        {
            if (s != null && s.CanSeek) s.Seek(0, SeekOrigin.Begin);
        }
        catch { }
    }

    public async Task<(List<Op> Ops, long OldLen, long NewLen, string OldTempPath, string NewTempPath)> DiffWithDiagnosticsAsync(
    PipeReader oldReader,
    PipeReader newReader,
    bool keepTempFiles = false)
    {
        string oldTemp = Path.GetTempFileName();
        string newTemp = Path.GetTempFileName();

        try
        {
            await CopyPipeToFileAsync(oldReader, oldTemp).ConfigureAwait(false);
            await CopyPipeToFileAsync(newReader, newTemp).ConfigureAwait(false);

            using (var oldFs = new FileStream(oldTemp, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var newFs = new FileStream(newTemp, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                long oldLen = oldFs.Length;
                long newLen = newFs.Length;

                var matches = new SortedDictionary<int, Match>();

                for (int pass = 0; pass < BLOCK_SIZES.Length; pass++)
                {
                    int blockSize = BLOCK_SIZES[pass];
                    int stride = STRIDES[pass];

                    if (oldLen < blockSize || newLen < blockSize) continue;

                    var index = BuildIndexFromFile(oldFs, blockSize);

                    for (int newPos = 0; newPos + blockSize <= newLen; newPos += stride)
                    {
                        ulong h = await HashBytesFromFileAsync(newFs, newPos, blockSize).ConfigureAwait(false);
                        if (!index.TryGetValue(h, out var candidates)) continue;

                        foreach (int oldPos in candidates)
                        {
                            if (IsOverlappingWithExisting(oldPos, blockSize, matches) || IsNewOverlapping(newPos, blockSize, matches))
                                continue;

                            if (await ExactMatchFileAsync(oldFs, oldPos, newFs, newPos, blockSize).ConfigureAwait(false))
                            {
                                int extendBack = await ExtendBackwardFileAsync(oldFs, newFs, oldPos, newPos).ConfigureAwait(false);
                                int oldStart = oldPos - extendBack;
                                int newStart = newPos - extendBack;

                                int extendForward = await ExtendForwardFileAsync(oldFs, newFs, oldPos, newPos, blockSize).ConfigureAwait(false);
                                int fullLen = extendBack + blockSize + extendForward;

                                if (!matches.ContainsKey(newStart))
                                    matches[newStart] = new Match(oldStart, fullLen);
                                else
                                {
                                    var existing = matches[newStart];
                                    if (fullLen > existing.Length)
                                        matches[newStart] = new Match(oldStart, fullLen);
                                }

                                newPos = Math.Max(newPos, newStart + fullLen - stride);
                                break;
                            }
                        }
                    }
                }

                var ops = new List<Op>();
                long cursorNew = 0;
                var enumerator = matches.GetEnumerator();
                bool hasEntry = enumerator.MoveNext();
                while (cursorNew < newLen)
                {
                    if (hasEntry)
                    {
                        var entry = enumerator.Current;
                        int matchNewPos = entry.Key;
                        var m = entry.Value;
                        if (matchNewPos < cursorNew)
                        {
                            hasEntry = enumerator.MoveNext();
                            continue;
                        }
                        if (matchNewPos > cursorNew)
                        {
                            int insLen = matchNewPos - (int)cursorNew;
                            byte[] insData = ReadBytesFromFile(newFs, cursorNew, insLen);
                            ops.Add(new Insert(cursorNew, insData));
                            cursorNew = matchNewPos;
                        }
                        ops.Add(new Copy(m.OldOffset, matchNewPos, m.Length));
                        cursorNew += m.Length;
                        hasEntry = enumerator.MoveNext();
                    }
                    else
                    {
                        if (cursorNew < newLen)
                        {
                            int insLen = (int)(newLen - cursorNew);
                            byte[] insData = ReadBytesFromFile(newFs, cursorNew, insLen);
                            ops.Add(new Insert(cursorNew, insData));
                        }
                        break;
                    }
                }

                var deletes = ComputeDeletesFromCopies(ops, (int)oldLen);
                ops.AddRange(deletes);
                var merged = MergeAdjacentOps(ops);

                // --- Diagnostics & sanity checks ---
                // expected new length
                long sumInserts = merged.OfType<Insert>().Sum(i => (long)i.Data.Length);
                long sumDeletes = merged.OfType<Delete>().Sum(d => d.Length);
                long expectedNewLen = oldLen - sumDeletes + sumInserts;

                if (expectedNewLen != newLen)
                {
                    Console.Error.WriteLine($"[DiffEngine] WARNING expectedNewLen ({expectedNewLen}) != actual new file len ({newLen}).");
                }

                // Validate copy ranges
                for (int i = 0; i < merged.Count; i++)
                {
                    if (merged[i] is Copy c)
                    {
                        if (c.OldOffset < 0 || c.OldOffset + c.Length > oldLen || c.NewOffset < 0 || c.NewOffset + c.Length > newLen)
                        {
                            Console.Error.WriteLine($"[DiffEngine] INVALID COPY op #{i}: {c}. oldLen={oldLen} newLen={newLen}");
                            // dump a small hex context for debugging
                            try
                            {
                                byte[] sampleOld = ReadBytesFromFile(oldFs, Math.Max(0, (int)c.OldOffset), Math.Min(64, c.Length));
                                byte[] sampleNew = ReadBytesFromFile(newFs, Math.Max(0, (int)c.NewOffset), Math.Min(64, c.Length));
                                Console.Error.WriteLine($"  sampleOld: {BitConverter.ToString(sampleOld)}");
                                Console.Error.WriteLine($"  sampleNew: {BitConverter.ToString(sampleNew)}");
                            }
                            catch { /* best-effort */ }
                        }
                    }
                }

                // Optionally keep temp files for offline debugging
                if (!keepTempFiles)
                {
                    TryDeleteTemp(oldTemp);
                    TryDeleteTemp(newTemp);
                    oldTemp = null;
                    newTemp = null;
                }
                else
                {
                    Console.Error.WriteLine($"[DiffEngine] Debug temp files kept: old='{oldTemp}', new='{newTemp}'");
                }

                return (merged, oldLen, newLen, oldTemp, newTemp);
            }
        }
        catch
        {
            // if exception, do not swallow — try to keep files for debugging if present
            Console.Error.WriteLine("[DiffEngine] Exception during DiffWithDiagnosticsAsync — preserving temp files for inspection.");
            Console.Error.WriteLine($"  oldTemp={oldTemp} newTemp={newTemp}");
            keepTempFiles = true;
            throw;
        }
        finally
        {
            if (keepTempFiles)
            {
                // copy to exe dir with timestamped names
                string timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss_fff");

                if (oldTemp != null && File.Exists(oldTemp))
                {
                    string destOld = Path.Combine(AppContext.BaseDirectory, $"diff_old_{timestamp}.tmp");
                    try { File.Copy(oldTemp, destOld, true); Console.Error.WriteLine($"[DiffEngine] Copied old temp to '{destOld}'"); }
                    catch { }
                }

                if (newTemp != null && File.Exists(newTemp))
                {
                    string destNew = Path.Combine(AppContext.BaseDirectory, $"diff_new_{timestamp}.tmp");
                    try { File.Copy(newTemp, destNew, true); Console.Error.WriteLine($"[DiffEngine] Copied new temp to '{destNew}'"); }
                    catch { }
                }
            }
            if (oldTemp != null && File.Exists(oldTemp)) TryDeleteTemp(oldTemp);
            if (newTemp != null && File.Exists(newTemp)) TryDeleteTemp(newTemp);
        }
    }
}
