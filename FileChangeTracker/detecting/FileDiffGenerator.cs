using FileChangeTracker.ops;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//var exeDir = PathLast(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location));
//instructions.Insert(0, new SetFileInstruction(PathFrom(path: actualFile, from: exeDir)));

//return PostProcessInstructions(instructions);
namespace FileChangeTracker.detecting
{
    public static class FileDiffGenerator
    {
        private const int CHUNK_SIZE = 4;  // You can adjust this size to optimize performance

        public static List<FileChangeInstruction> Diff(FileStream originalStream, FileStream modifiedStream, string actualFile)
        {
            List<FileChangeInstruction> changes = new();
            byte[] originalBuffer = new byte[CHUNK_SIZE];
            byte[] modifiedBuffer = new byte[CHUNK_SIZE];

            long originalPosition = 0;
            long modifiedPosition = 0;

            while (true)
            {
                int originalBytesRead = originalStream.Read(originalBuffer, 0, CHUNK_SIZE);
                int modifiedBytesRead = modifiedStream.Read(modifiedBuffer, 0, CHUNK_SIZE);

                // If no data is left in both files, we're done.
                if (originalBytesRead == 0 && modifiedBytesRead == 0)
                    break;

                int originalIndex = 0, modifiedIndex = 0;
                while (originalIndex < originalBytesRead && modifiedIndex < modifiedBytesRead)
                {
                    if (originalBuffer[originalIndex] == modifiedBuffer[modifiedIndex])
                    {
                        originalIndex++;
                        modifiedIndex++;
                    }
                    else
                    {
                        // No match within this chunk, so we assume insertion or deletion
                        int chunkLength = GetLengthWithoutTrailingZeros(modifiedBuffer);
                        byte[] chunkData = new byte[chunkLength];
                        Array.Copy(modifiedBuffer, chunkData, chunkLength);
                        changes.Add(new DeleteInstruction(modifiedPosition + modifiedIndex, chunkLength));
                        changes.Add(new InsertInstruction(modifiedPosition + modifiedIndex, chunkData));
                        modifiedIndex += chunkLength;
                    }
                }

                originalPosition += originalBytesRead;
                modifiedPosition += modifiedBytesRead;
            }

            var exeDir = PathLast(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location));
            changes.Insert(0, new SetFileInstruction(PathFrom(path: actualFile, from: exeDir)));

            return PostProcessInstructions(changes);
        }
        public static int GetLengthWithoutTrailingZeros(byte[] data)
        {
            if (data == null || data.Length == 0)
                return 0;

            int i = data.Length - 1;
            while (i >= 0 && data[i] == 0)
                i--;

            return i + 1;
        }

        private static List<FileChangeInstruction> PostProcessInstructions(List<FileChangeInstruction> instructions)
        {
            var processedInstructions = TransformSequentialInsertDelete(instructions);
            processedInstructions = MergeSequentialUpdates(processedInstructions);
            processedInstructions = MergeSequentialDeletes(processedInstructions);
            processedInstructions = MergeSequentialInserts(processedInstructions);
            return processedInstructions;
        }
        private static List<FileChangeInstruction> TransformSequentialInsertDelete(List<FileChangeInstruction> instructions)
        {
            var processedInstructions = new List<FileChangeInstruction>();
            int i = 0;

            while (i < instructions.Count)
            {
                DeleteInstruction delete;
                InsertInstruction insert;

                if (TryTransformToUpdateAndTrailingDelete(instructions, i, out delete, out insert))
                {
                    int deleteLength = BitConverter.ToInt32(delete.data!);
                    int insertLength = insert.data!.Length;

                    // Add UPDATE
                    processedInstructions.Add(new UpdateInstruction(delete.offset, insert.data));

                    // Add trailing DELETE if needed
                    int trailingDeleteLength = deleteLength - insertLength;
                    if (trailingDeleteLength > 0)
                        processedInstructions.Add(new DeleteInstruction(delete.offset + insertLength, trailingDeleteLength));

                    i += 2; // Skip the pair
                }
                else
                {
                    processedInstructions.Add(instructions[i]);
                    i++;
                }
            }

            return processedInstructions;

            static bool TryTransformToUpdateAndTrailingDelete(List<FileChangeInstruction> instructions, int i, out DeleteInstruction delete, out InsertInstruction insert)
            {
                delete = null!;
                insert = null!;

                if (i + 1 >= instructions.Count)
                    return false;

                if (instructions[i] is not DeleteInstruction del || instructions[i + 1] is not InsertInstruction ins)
                    return false;

                if (del.offset != ins.offset)
                    return false;

                int deleteLength = BitConverter.ToInt32(del.data!);
                int insertLength = ins.data!.Length;

                if (insertLength > deleteLength)
                    return false;

                delete = del;
                insert = ins;
                return true;
            }
        }
        private static List<FileChangeInstruction> MergeSequentialUpdates(List<FileChangeInstruction> instructions)
        {
            var result = new List<FileChangeInstruction>();
            int i = 0;

            while (i < instructions.Count)
            {
                if (instructions[i] is UpdateInstruction currentUpdate)
                {
                    long mergedOffset = currentUpdate.offset;
                    List<byte> mergedData = [.. currentUpdate.data];

                    int j = i + 1;
                    while (j < instructions.Count && instructions[j] is UpdateInstruction nextUpdate)
                    {
                        // Only merge if the next update is immediately after the current update
                        if (nextUpdate.offset == mergedOffset + mergedData.Count)
                        {
                            mergedData.AddRange(nextUpdate.data);
                            j++;
                        }
                        else
                        {
                            break;
                        }
                    }

                    result.Add(new UpdateInstruction(mergedOffset, mergedData.ToArray()));
                    i = j;
                }
                else
                {
                    result.Add(instructions[i]);
                    i++;
                }
            }

            return result;
        }
        private static List<FileChangeInstruction> MergeSequentialDeletes(List<FileChangeInstruction> instructions)
        {
            var result = new List<FileChangeInstruction>();
            int i = 0;

            while (i < instructions.Count)
            {
                if (instructions[i] is DeleteInstruction currentDelete)
                {
                    long mergedOffset = currentDelete.offset;
                    int mergedLength = BitConverter.ToInt32(currentDelete.data!);

                    int j = i + 1;
                    while (j < instructions.Count && instructions[j] is DeleteInstruction nextDelete)
                    {
                        long nextOffset = nextDelete.offset;
                        int nextLength = BitConverter.ToInt32(nextDelete.data!);

                        // Only merge if the next delete is immediately after the current delete
                        if (nextOffset == mergedOffset + mergedLength)
                        {
                            mergedLength += nextLength;
                            j++;
                        }
                        else
                        {
                            break;
                        }
                    }

                    result.Add(new DeleteInstruction(mergedOffset, mergedLength));
                    i = j;
                }
                else
                {
                    result.Add(instructions[i]);
                    i++;
                }
            }

            return result;
        }
        private static List<FileChangeInstruction> MergeSequentialInserts(List<FileChangeInstruction> instructions)
        {
            var result = new List<FileChangeInstruction>();
            int i = 0;

            while (i < instructions.Count)
            {
                if (instructions[i] is InsertInstruction currentInsert)
                {
                    long mergedOffset = currentInsert.offset;
                    List<byte> mergedData = new List<byte>(currentInsert.data);

                    int j = i + 1;
                    while (j < instructions.Count && instructions[j] is InsertInstruction nextInsert)
                    {
                        if (nextInsert.offset == mergedOffset + mergedData.Count)
                        {
                            mergedData.AddRange(nextInsert.data);
                            j++;
                        }
                        else
                        {
                            break;
                        }
                    }

                    result.Add(new InsertInstruction(mergedOffset, mergedData.ToArray()));
                    i = j;
                }
                else
                {
                    result.Add(instructions[i]);
                    i++;
                }
            }

            return result;
        }

        private static string PathLast(string path)
        {
            path = path.Replace('/', '\\');
            var pathPoints = path.Split('\\', StringSplitOptions.RemoveEmptyEntries);
            return pathPoints.Last();
        }

        private static string PathFrom(string path, string from)
        {
            path = path.Replace('/', '\\');
            var pathPoints = path.Split('\\', StringSplitOptions.RemoveEmptyEntries);
            int fromIndex = pathPoints.ToList().IndexOf(from);
            if (fromIndex is -1)
                return path;

            var newPathPoints = pathPoints[++fromIndex..];
            var newPath = Path.Combine(newPathPoints);
            return newPath;
        }
    }
    //internal static class FileStreamExtensions
    //{
    //    public static byte ReadByteAt(this FileStream stream, long position)
    //    {
    //        long currentPos = stream.Position;
    //        stream.Position = position;
    //        int b = stream.ReadByte();
    //        stream.Position = currentPos;
    //        return (byte)b;
    //    }
    //}
}
