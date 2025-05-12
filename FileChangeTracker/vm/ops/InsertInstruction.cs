using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FileChangeTracker
{
    public class InsertInstruction : FileChangeInstruction
    {
        public override FileChangeOpcode opcode => FileChangeOpcode.INSERT;

        public InsertInstruction(long offset, byte[] data)
        {
            base.offset = offset;
            base.data = data;
        }

        public override void Execute(FileChangeContext context)
        {
            var stream = context.CurrentStream;
            long originalLength = stream.Length;
            long insertLength = data!.Length;

            // Extend the stream to make room
            stream.SetLength(originalLength + insertLength);

            // Move data backwards to make space for insertion
            const int BUFFER_SIZE = 4096;
            byte[] buffer = new byte[BUFFER_SIZE];

            long readPos = originalLength;
            long writePos = originalLength + insertLength;

            while (readPos > offset)
            {
                int chunkSize = (int)Math.Min(BUFFER_SIZE, readPos - offset);
                readPos -= chunkSize;
                writePos -= chunkSize;

                stream.Position = readPos;
                stream.ReadExactly(buffer, 0, chunkSize);

                stream.Position = writePos;
                stream.Write(buffer, 0, chunkSize);
            }

            // Now write the inserted data
            stream.Position = offset;
            stream.Write(data, 0, data.Length);
        }

    }
}
