using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Text;
using System.Diagnostics.CodeAnalysis;

namespace WinterRose.FileManagement
{
    /// <summary>
    /// A class to read large files asynchronously, in parallel and is memory intensive. 
    /// </summary>
    [Experimental("WR_NOTIMPLEMENTED")]
    public class LargeFileReader
    {
        private List<Memory<char>> pages = new List<Memory<char>>();
        private const int PAGE_SIZE = 1024 * 1024;
        private int threads;

        /// <summary>
        /// Default constructor for LargeFileReader. Uses the number of processors available as the default number of threads used.
        /// </summary>
        public LargeFileReader() : this(Environment.ProcessorCount) { }

        /// <summary>
        /// Constructor for LargeFileReader. Accepts an integer representing the number of threads you want to use.
        /// </summary>
        /// <param name="threads">Integer representing the number of threads to use</param>
        public LargeFileReader(int threads)
        {
            pages = new List<Memory<char>>();
            this.threads = threads;
        }

        /// <summary>
        /// Helper method called for every chunk of the file to be read. Takes a FileStream object and boundaries of the chunk
        /// to read in bytes as parameters. Passes it on to the File stream to read asynchronously. Returns a char Memory object.
        /// </summary>
        /// <param name="file">A FileStream object representing the file to be read.</param>
        /// <param name="readStart">A long representing the starting position of the chunk to be read.</param>
        /// <param name="readEnd">A long representing the ending position of the chunk to be read.</param>
        /// <returns>A Memory of char type.</returns>
        private async Task<Memory<char>> ReadFileChunksAsync(FileStream file, long readStart, long readEnd)
        {
            int chunkSize = (int)(readEnd - readStart);
            byte[] buffer = new byte[chunkSize];

            file.Seek(readStart, SeekOrigin.Begin);
            await file.ReadAsync(buffer, 0, chunkSize);

            Decoder decoder = Encoding.UTF8.GetDecoder();
            int charCount = decoder.GetCharCount(buffer, 0, chunkSize);

            var charBuffer = new char[charCount];
            decoder.GetChars(buffer, 0, chunkSize, charBuffer, 0);

            Memory<char> chunk = new Memory<char>(charBuffer);
            return chunk;
        }

        /// <summary>
        /// Reads a large file in parallel using all available threads. Returns the current LargeFileReader object for method chaining.
        /// </summary>
        /// <param name="filePath">A string representing the path of the file to be read.</param>
        /// <returns>The current LargeFileReader object for method chaining.</returns>
        public LargeFileReader ReadLargeFile(string filePath)
        {
            FileInfo fileInfo = new FileInfo(filePath);
            long fileSize = fileInfo.Length;

            int totalPages = (int)Math.Ceiling((double)fileSize / PAGE_SIZE);
            int threadsPerChunk = (int)Math.Ceiling((double)threads / totalPages);

            var tasks = new List<Task>();

            using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, PAGE_SIZE, FileOptions.SequentialScan))
            {
                for (int i = 0; i < totalPages; i++)
                {
                    long readStart = i * (long)PAGE_SIZE;
                    long readEnd = Math.Min(readStart + PAGE_SIZE, fileSize);

                    tasks.Add(Task.Run(async () =>
                    {
                        Memory<char> chunk = await ReadFileChunksAsync(fileStream, readStart, readEnd);
                        lock (pages)
                        {
                            pages.Add(chunk);
                        }
                    }));
                }

                Task.WaitAll(tasks.ToArray());
            }

            return this;
        }
    }
}


