using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileChangeTracker
{
    public class SetFileInstruction : FileChangeInstruction
    {
        public override FileChangeOpcode opcode => FileChangeOpcode.SETFILE;
        public SetFileInstruction(string path) => base.path = path;
        public override void Execute(FileChangeContext context) => context.SetFile(path!);
    }
}
