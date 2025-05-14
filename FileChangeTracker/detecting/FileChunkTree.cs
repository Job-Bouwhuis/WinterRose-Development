using FileChangeTracker.ops;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace FileChangeTracker.detecting
{
    public class FileChunkTree
    {
        public const int BASE_CHUNK_SIZE = 32;
        public const int HASH_GROUP_SIZE = 10;

        public List<List<byte[]>> Levels { get; } = new();

        public static FileChunkTree BuildFromFile(string filePath)
        {
            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            return BuildFromStream(stream);
        }

        public static FileChunkTree BuildFromStream(Stream stream)
        {
            var tree = new FileChunkTree();

            // Phase 1: read base-level chunks
            List<byte[]> baseChunks = new();
            var buffer = new byte[BASE_CHUNK_SIZE];
            int bytesRead;
            while ((bytesRead = stream.Read(buffer, 0, BASE_CHUNK_SIZE)) > 0)
            {
                byte[] actualChunk = (bytesRead == BASE_CHUNK_SIZE)
                    ? buffer.ToArray()
                    : buffer.Take(bytesRead).ToArray();

                baseChunks.Add(HashBytes(actualChunk));
            }

            tree.Levels.Add(baseChunks);

            // Phase 2: build zoom levels
            while (tree.Levels.Last().Count > 1)
            {
                var prevLevel = tree.Levels.Last();
                List<byte[]> newLevel = new();

                for (int i = 0; i < prevLevel.Count; i += HASH_GROUP_SIZE)
                {
                    var group = prevLevel
                        .Skip(i)
                        .Take(HASH_GROUP_SIZE)
                        .SelectMany(h => h)
                        .ToArray();

                    newLevel.Add(HashBytes(group));
                }

                tree.Levels.Add(newLevel);
            }

            return tree;
        }

        private static byte[] HashBytes(byte[] data)
        {
            using var sha = SHA256.Create();
            return sha.ComputeHash(data);
        }

        public List<ChunkDifference> CompareWith(FileChunkTree other)
        {
            var differences = new List<ChunkDifference>();

            int thisTopLevel = Levels.Count - 1;
            int otherTopLevel = other.Levels.Count - 1;

            int commonTop = Math.Min(thisTopLevel, otherTopLevel);

            CompareLevelRecursive(
                thisLevel: thisTopLevel,
                otherLevel: otherTopLevel,
                thisIndex: 0,
                otherIndex: 0,
                logicalChunkIndex: 0,
                zoom: commonTop,
                differences,
                other
            );

            return differences;
        }

        private void CompareLevelRecursive(
            int thisLevel,
            int otherLevel,
            int thisIndex,
            int otherIndex,
            int logicalChunkIndex,
            int zoom,
            List<ChunkDifference> differences,
            FileChunkTree otherTree
        )
        {
            var thisHashes = Levels[thisLevel];
            var otherHashes = otherTree.Levels[otherLevel];

            int count = Math.Min(thisHashes.Count, otherHashes.Count);

            for (int i = 0; i < count; i++)
            {
                byte[] thisHash = thisHashes[i];
                byte[] otherHash = otherHashes[i];

                if (!thisHash.SequenceEqual(otherHash))
                {
                    if (zoom == 0)
                    {
                        // At base level, record the difference
                        int absoluteChunkIndex = logicalChunkIndex + i;
                        differences.Add(new ChunkDifference(
                            i,
                            thisHash,
                            otherHash
                        ));
                    }
                    else
                    {
                        // Need to dive deeper into this mismatching group
                        int childStart = i * HASH_GROUP_SIZE;
                        CompareLevelRecursive(
                            thisLevel - 1,
                            otherLevel - 1,
                            childStart,
                            childStart,
                            logicalChunkIndex + i * (int)Math.Pow(HASH_GROUP_SIZE, zoom - 1),
                            zoom - 1,
                            differences,
                            otherTree
                        );
                    }
                }
            }

            // Handle size mismatch (if one file is longer than the other)
            if (thisHashes.Count != otherHashes.Count && zoom == 0)
            {
                int longer = Math.Max(thisHashes.Count, otherHashes.Count);
                for (int i = count; i < longer; i++)
                {
                    int index = logicalChunkIndex + i;
                    byte[]? thisHash = i < thisHashes.Count ? thisHashes[i] : null;
                    byte[]? otherHash = i < otherHashes.Count ? otherHashes[i] : null;

                    differences.Add(new ChunkDifference(index, thisHash, otherHash));
                }
            }
        }

        public static List<FileChangeInstruction> GenerateInstructions(
            FileChunkTree oldTree,
            FileChunkTree newTree,
            string oldFilePath,
            string newFilePath,
            string actualFile
        )
        {
            using var oldStream = new FileStream(oldFilePath, FileMode.Open, FileAccess.Read);
            using var newStream = new FileStream(newFilePath, FileMode.Open, FileAccess.Read);
            return GenerateInstructions(oldTree, newTree, oldStream, newStream, actualFile);
        }

        public static List<FileChangeInstruction> GenerateInstructions(
            FileChunkTree oldTree,
            FileChunkTree newTree,
            Stream oldStream,
            Stream newStream,
            string actualFile
        )
        {
            var differences = oldTree.CompareWith(newTree);
            var instructions = new List<FileChangeInstruction>();

            foreach (var diff in differences)
            {
                long byteOffset = diff.ChunkIndex * BASE_CHUNK_SIZE;

                byte[] oldData = byteOffset < oldStream.Length
                    ? ReadChunk(oldStream, byteOffset, BASE_CHUNK_SIZE)
                    : Array.Empty<byte>();

                byte[] newData = byteOffset < newStream.Length
                    ? ReadChunk(newStream, byteOffset, BASE_CHUNK_SIZE)
                    : Array.Empty<byte>();

                // Now we explicitly know what's missing
                if (oldData.Length == 0 && newData.Length > 0)
                {
                    // If data is present in the new file but not in the old one, insert the new data.
                    instructions.Add(new InsertInstruction(byteOffset, newData));
                }
                else if (newData.Length == 0 && oldData.Length > 0)
                {
                    // If data is present in the old file but not in the new one, delete the old data.
                    instructions.Add(new DeleteInstruction(byteOffset, oldData.Length));
                }
                else if (!oldData.SequenceEqual(newData))
                {
                    // If the chunks are different, delete the old data and insert the new data.
                    instructions.Add(new DeleteInstruction(byteOffset, oldData.Length));
                    instructions.Add(new InsertInstruction(byteOffset, newData));
                }
            }

            return InstructionPostProcessor.PostProcessInstructions(instructions, actualFile);
        }

        private static byte[] ReadChunk(Stream stream, long offset, int count)
        {
            if (offset >= stream.Length)
                return Array.Empty<byte>();

            long maxBytes = Math.Min(count, stream.Length - offset);
            byte[] buffer = new byte[maxBytes];

            long original = stream.Position;
            stream.Seek(offset, SeekOrigin.Begin);
            stream.Read(buffer, 0, buffer.Length);
            stream.Seek(original, SeekOrigin.Begin); // Optional: restore original pos

            return buffer;
        }
    }
}
