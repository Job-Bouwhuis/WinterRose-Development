using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileChangeTracker
{
    public class DeleteInstruction : FileChangeInstruction
    {
        public override FileChangeOpcode opcode => FileChangeOpcode.DELETE;

        public DeleteInstruction(long offset, int count)
        {
            base.offset = offset;
            data = new byte[4];
            BitConverter.GetBytes(count).CopyTo(data, 0);
        }

        public override void Execute(FileChangeContext context)
        {
            var stream = context.CurrentStream;

            int length = BitConverter.ToInt32(data);
            int maxDeletable = (int)Math.Max(0, stream.Length - offset);
            length = Math.Min(length, maxDeletable);

            long readPos = offset + length;
            long remaining = stream.Length - readPos;
            if (remaining <= 0)
            {
                stream.SetLength(Math.Max(0, stream.Length - length));
                return;
            }

            byte[] trailingData = new byte[remaining];
            stream.Position = readPos;
            stream.ReadExactly(trailingData, 0, trailingData.Length);

            stream.Position = offset;
            stream.Write(trailingData, 0, trailingData.Length);

            stream.SetLength(Math.Max(0, stream.Length - length));
        }

    }
}
