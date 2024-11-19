using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose;
using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using System.Threading;

public class File256
{
    private Array256<byte> byteArray = new();

    public File256()
    {
    }

    // Method to read the file into the Array256
    public void ReadFile(string filePath)
    {
        using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
        {
            long fileLength = fs.Length;
            int chunkSize = 1024 * 1024; // 1 MB chunks
            byte[] buffer = new byte[chunkSize];

            int numChunks = (int)Math.Ceiling((double)fileLength / chunkSize);
            List<Task> tasks = new List<Task>();

            // Read the file in parallel chunks
            for (int chunkIndex = 0; chunkIndex < numChunks; chunkIndex++)
            {
                long startOffset = chunkIndex * chunkSize;
                long endOffset = Math.Min(startOffset + chunkSize, fileLength);

                // Calculate the number of bytes to read for this chunk
                int bytesToRead = (int)(endOffset - startOffset);
                byte[] chunkBuffer = new byte[bytesToRead];

                tasks.Add(Task.Run(() =>
                {
                    // Read the chunk into the buffer
                    fs.Seek(startOffset, SeekOrigin.Begin);
                    fs.Read(chunkBuffer, 0, bytesToRead);

                    // Write the chunk into byteArray
                    for (int i = 0; i < bytesToRead; i++)
                    {
                        Int256 index = new Int256((ulong)(startOffset + i), 0, 0, 0);
                        lock (byteArray)  // Ensure thread safety when writing to the Array256
                        {
                            byteArray[index] = chunkBuffer[i];
                        }
                    }
                }));
            }

            // Wait for all tasks to complete
            Task.WhenAll(tasks).Wait();
        }
    }


    public void WriteFile(string filePath)
    {
        using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
        {
            long position = 0;
            byte[] buffer = new byte[1024 * 1024];  // 1 MB buffer

            // Iterate over the Array256 and write chunks to the file
            while (true)
            {
                // Create a chunk to hold the data
                int bufferIndex = 0;

                // Iterate over the chunk, filling the buffer
                for (long i = position; i < position + buffer.Length; i++)
                {
                    Int256 index = new Int256((ulong)i, 0, 0, 0);

                    // If the current index exists in the byteArray, assign the byte to the buffer
                    lock (byteArray)
                    {
                        if (byteArray.ContainsKey(index)) // Check if the index exists
                        {
                            buffer[bufferIndex++] = byteArray[index];
                        }
                        else
                        {
                            // No more data to write, break out of the loop
                            if (bufferIndex == 0)
                            {
                                return;  // No more data to write
                            }
                        }
                    }
                }

                // Write the filled buffer to the file
                fs.Write(buffer, 0, bufferIndex);

                // Update the position for the next chunk
                position += bufferIndex;
            }
        }
    }
}

