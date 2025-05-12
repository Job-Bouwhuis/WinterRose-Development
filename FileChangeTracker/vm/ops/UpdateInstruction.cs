using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileChangeTracker.ops
{
    public class UpdateInstruction : FileChangeInstruction
    {
        public override FileChangeOpcode opcode => FileChangeOpcode.UPDATE;

        public UpdateInstruction(long offset, byte[] data)
        {
            base.offset = offset;
            base.data = data;
        }

        public override void Execute(FileChangeContext context)
        {
            var stream = context.CurrentStream;

            if (offset + data.Length > stream.Length)
                stream.SetLength(offset + data.Length);

            stream.Position = offset;
            stream.Write(data, 0, data.Length);
        }
    }
}
