using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileChangeTracker.ops
{
    public class DeleteFileInstruction : FileChangeInstruction
    {
        public override FileChangeOpcode opcode => FileChangeOpcode.DELFILE;
        public DeleteFileInstruction(string path) => base.path = path;
        public override void Execute(FileChangeContext context) => context.DeleteFile(path!);
    }
}
