using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WinterRose.WinterForgeSerializing;

namespace WinterRose.Diff
{
    public class DiffApplyer
    {
        public bool ApplyDiff(string filePath, FileDiff diff)
        {
            try
            {
                if (diff.State == FileState.Added)
                {
                    File.Create(filePath);
                    diff.State = FileState.Modified;
                }

                if (diff.State == FileState.Deleted)
                {
                    if (File.Exists(filePath))
                        File.Delete(filePath);
                }

                if (diff.State == FileState.Modified)
                {
                    using FileStream file = File.Open(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);

                    // Ops are in old-file space; track cumulative byte shift as we apply
                    long delta = 0;

                    foreach (var op in diff.Operations)
                    {
                        if (op is DiffEngine.Insert insert)
                        {
                            // Shift offset into current live-file space
                            var shifted = new DiffEngine.Insert(insert.Offset + delta, insert.Data);
                            ApplyInsert(file, shifted);
                            delta += insert.Data.Length;
                        }
                        else if (op is DiffEngine.Delete delete)
                        {
                            var shifted = new DiffEngine.Delete(delete.Offset + delta, delete.Length);
                            ApplyDelete(file, shifted);
                            delta -= delete.Length;
                        }
                    }
                }
                return true;
            }
            catch (Exception ex) 
            {
                return false;
            }
        }

        private void ApplyInsert(FileStream file, DiffEngine.Insert insert)
        {
            long offset = insert.Offset;
            byte[] data = insert.Data;

            file.Seek(0, SeekOrigin.End);
            long end = file.Position;

            long moveSize = end - offset;

            byte[] buffer = new byte[8192];

            long readPos = end;
            long writePos = end + data.Length;

            while (moveSize > 0)
            {
                int chunkSize = (int)Math.Min(buffer.Length, moveSize);

                readPos -= chunkSize;
                file.Position = readPos;
                file.ReadExactly(buffer.AsSpan(0, chunkSize));

                writePos -= chunkSize;
                file.Position = writePos;
                file.Write(buffer.AsSpan(0, chunkSize));

                moveSize -= chunkSize;
            }

            file.Position = offset;
            file.Write(data);
        }

        private void ApplyDelete(FileStream file, DiffEngine.Delete delete)
        {
            long offset = delete.Offset;
            long length = delete.Length;

            file.Seek(0, SeekOrigin.End);
            long end = file.Position;

            long readPos = offset + length;
            long writePos = offset;

            byte[] buffer = new byte[8192];

            long remaining = end - readPos;

            while (remaining > 0)
            {
                int chunkSize = (int)Math.Min(buffer.Length, remaining);

                file.Position = readPos;
                file.ReadExactly(buffer.AsSpan(0, chunkSize));

                file.Position = writePos;
                file.Write(buffer.AsSpan(0, chunkSize));

                readPos += chunkSize;
                writePos += chunkSize;
                remaining -= chunkSize;
            }

            file.SetLength(end - length);
        }
    }
}