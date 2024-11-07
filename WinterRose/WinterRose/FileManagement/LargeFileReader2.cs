using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.FileManagement
{
    /// <summary>
    /// Not Implemented
    /// </summary>
    [Experimental("WR_NOTIMPLEMENTED")]
    public class LargeFileReader2 : IDisposable
    {
        private const int PAGE_SIZE = 1024 * 1024;
        private readonly MemoryPool<char> memoryPool;
        private readonly List<Memory<char>> pages = new List<Memory<char>>();
        private int threads;

        /// <summary>
        /// Default constructor for LargeFileReader. Uses the number of processors available as the default number of threads used.
        /// </summary>
        public LargeFileReader2() : this(Environment.ProcessorCount) { }

        /// <summary>
        /// Constructor for LargeFileReader. Accepts an integer representing the number of threads you want to use.
        /// </summary>
        /// <param name="threads">Integer representing the number of threads to use</param>
        public LargeFileReader2(int threads)
        {
            throw new NotImplementedException();
            this.threads = threads;
            memoryPool = MemoryPool<char>.Shared;
        }

        /// <summary>
        /// Helper method called for every chunk of the file to be read. Takes a FileStream object and boundaries of the chunk
        /// to read in bytes as parameters. Passes it on to the File stream to read asynchronously. Returns a char Memory object.
        /// </summary>
        /// <param name="file">A FileStream object representing the file to be read.</param>
        /// <param name="readStart">A long representing the starting position of the chunk to be read.</param>
        /// <param name="readEnd">A long representing the ending position of the chunk to be read.</param>
        /// <returns>A Memory object of char type.</returns>
        private async Task<Memory<char>> ReadFileChunksAsync(FileStream file, long readStart, long readEnd)
        {
            int chunkSize = (int)(readEnd - readStart);
            byte[] buffer = new byte[chunkSize];
            file.Seek(readStart, SeekOrigin.Begin);
            await file.ReadAsync(buffer, 0, chunkSize);

            Memory<char> memory = memoryPool.Rent(chunkSize).Memory;
            Decoder decoder = Encoding.UTF8.GetDecoder();
            int charCount = decoder.GetCharCount(buffer, 0, chunkSize);

            char[] charBuffer = memory.Span.Slice(0, charCount).ToArray();
            decoder.GetChars(buffer, 0, chunkSize, charBuffer, 0);
            return memory;
        }

        /// <summary>
        /// Reads a large file in parallel using all available threads. Returns the current LargeFileReader object for method chaining.
        /// </summary>
        /// <param name="filePath">A string representing the path of the file to be read.</param>
        /// <returns>The current LargeFileReader object for method chaining.</returns>
        public LargeFileReader2 ReadLargeFile(string filePath)
        {
            FileInfo fileInfo = new FileInfo(filePath);
            long fileSize = fileInfo.Length;
            int totalPages = (int)Math.Ceiling((double)fileSize / PAGE_SIZE);
            int threadsPerChunk = (int)Math.Ceiling((double)threads / totalPages);

            using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, PAGE_SIZE, FileOptions.SequentialScan))
            {
                var tasks = new List<Task<Memory<char>>>();

                for (int i = 0; i < totalPages; i++)
                {
                    long readStart = i * (long)PAGE_SIZE;
                    long readEnd = Math.Min(readStart + PAGE_SIZE, fileSize);

                    tasks.Add(ReadFileChunksAsync(fileStream, readStart, readEnd));
                }

                Task.WaitAll(tasks.ToArray());

                foreach (var task in tasks)
                {
                    lock (pages)
                    {
                        pages.Add(task.Result);
                    }
                }
            }

            try
            {
                return this;
            }
            finally
            {
                6.Repeat(x => GC.Collect());
            }
        }

        public void WriteLargeFile(string filePath)
        {
            throw new NotImplementedException();
            //using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, PAGE_SIZE, FileOptions.WriteThrough))
            //{
            //    foreach (var page in pages)
            //    {
            //        var buffer = Encoding.UTF8.GetBytes(page.Span);
            //        fileStream.Write(buffer, 0, buffer.Length);
            //    }
            //}
        }


        public void Dispose()
        {
            memoryPool.Dispose();
        }
    }
}