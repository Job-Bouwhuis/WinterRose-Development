using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.Diff
{
    public static class DiffTracker
    {
        public static List<ByteDiff> GetSmartByteDiff(Stream original, Stream modified)
        {
            if (!original.CanSeek || !modified.CanSeek)
                throw new InvalidOperationException("Both streams must support seeking.");

            long lenA = original.Length;
            long lenB = modified.Length;

            var dpRows = ComputeLCSRows(original, lenA, modified, lenB);
            var lcs = ReconstructLCS(original, lenA, modified, lenB, dpRows);

            var diffs = new List<ByteDiff>();
            long i = 0, j = 0;

            foreach ((long oi, long mi) in lcs)
            {
                while (i < oi)
                {
                    diffs.Add(new ByteDiff
                    {
                        PositionOriginal = i,
                        Type = DiffType.Removed,
                        Value = ReadByteAt(original, i)
                    });
                    i++;
                }

                while (j < mi)
                {
                    diffs.Add(new ByteDiff
                    {
                        PositionModified = j,
                        Type = DiffType.Added,
                        Value = ReadByteAt(modified, j)
                    });
                    j++;
                }

                i++; j++; // skip matched byte
            }

            while (i < lenA)
            {
                diffs.Add(new ByteDiff
                {
                    PositionOriginal = i,
                    Type = DiffType.Removed,
                    Value = ReadByteAt(original, i)
                });
                i++;
            }

            while (j < lenB)
            {
                diffs.Add(new ByteDiff
                {
                    PositionModified = j,
                    Type = DiffType.Added,
                    Value = ReadByteAt(modified, j)
                });
                j++;
            }

            return diffs;
        }

        private static int[][] ComputeLCSRows(Stream a, long lenA, Stream b, long lenB)
        {
            int[] currentRow = new int[lenB + 1];
            int[] nextRow = new int[lenB + 1];

            for (long i = lenA - 1; i >= 0; i--)
            {
                Console.WriteLine(i);
                // Swap references so currentRow becomes nextRow for this iteration
                var temp = currentRow;
                currentRow = nextRow;
                nextRow = temp;

                for (long j = lenB - 1; j >= 0; j--)
                {
                    byte ai = ReadByteAt(a, i);
                    byte bj = ReadByteAt(b, j);

                    if (ai == bj)
                        currentRow[j] = 1 + nextRow[j + 1];
                    else
                        currentRow[j] = Math.Max(nextRow[j], currentRow[j + 1]);
                }
            }

            // Return currentRow (now at dp[0]) and nextRow for reconstruction
            return new[] { currentRow, nextRow };
        }

        private static List<(long, long)> ReconstructLCS(Stream a, long lenA, Stream b, long lenB, int[][] dpRows)
        {
            var lcs = new List<(long, long)>();
            long i = 0, j = 0;
            var currentRow = dpRows[0];
            var nextRow = dpRows[1];

            while (i < lenA && j < lenB)
            {
                byte ai = ReadByteAt(a, i);
                byte bj = ReadByteAt(b, j);

                if (ai == bj)
                {
                    lcs.Add((i, j));
                    i++;
                    j++;
                }
                else
                {
                    // For next step we need dp[i+1,j] and dp[i,j+1]
                    // But we only have currentRow (dp[i]) and nextRow (dp[i+1])
                    // dp[i+1,j] = nextRow[j]
                    // dp[i,j+1] = currentRow[j+1]

                    int valDown = (i + 1 < lenA) ? nextRow[(int)j] : 0;
                    int valRight = (j + 1 < lenB) ? currentRow[(int)(j + 1)] : 0;

                    if (valDown >= valRight)
                        i++;
                    else
                        j++;
                }
            }

            return lcs;
        }

        private static byte ReadByteAt(Stream stream, long position)
        {
            long oldPos = stream.Position;
            stream.Seek(position, SeekOrigin.Begin);
            int b = stream.ReadByte();
            stream.Seek(oldPos, SeekOrigin.Begin);
            if (b == -1)
                throw new EndOfStreamException($"Attempted to read beyond end of stream at {position}");
            return (byte)b;
        }
    }
}
