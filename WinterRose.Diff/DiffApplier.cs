using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.Diff
{
    public static class DiffApplier
    {
        public static void ApplyDiffs(Stream original, List<ByteDiff> diffs)
        {
            if (!original.CanSeek || !original.CanRead || !original.CanWrite)
                throw new InvalidOperationException("Stream must support seek, read, and write.");

            // Create a temporary stream (memory or file-backed if needed)
            using var temp = new MemoryStream();

            long originalLength = original.Length;
            long currentOriginalPos = 0;
            int diffIndex = 0;

            while (currentOriginalPos < originalLength || diffIndex < diffs.Count)
            {
                if (diffIndex < diffs.Count)
                {
                    var diff = diffs[diffIndex];

                    if (diff.Type == DiffType.Removed && diff.PositionOriginal == currentOriginalPos)
                    {
                        // skip the byte in original
                        currentOriginalPos++;
                        diffIndex++;
                        continue;
                    }

                    if (diff.Type == DiffType.Added && diff.PositionModified == temp.Position)
                    {
                        temp.WriteByte(diff.Value);
                        diffIndex++;
                        continue;
                    }
                }

                if (currentOriginalPos < originalLength)
                {
                    // copy byte from original to temp
                    original.Seek(currentOriginalPos, SeekOrigin.Begin);
                    int b = original.ReadByte();
                    if (b == -1)
                        break;
                    temp.WriteByte((byte)b);
                    currentOriginalPos++;
                }
                else
                {
                    break;
                }
            }

            // Now copy temp back to original
            original.SetLength(temp.Length);
            original.Seek(0, SeekOrigin.Begin);
            temp.Seek(0, SeekOrigin.Begin);
            temp.CopyTo(original);
            original.Seek(0, SeekOrigin.Begin);
        }
    }

}
