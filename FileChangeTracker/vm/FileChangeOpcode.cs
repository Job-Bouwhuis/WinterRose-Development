using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileChangeTracker
{
    public enum FileChangeOpcode : byte
    {
        INSERT,
        DELETE,
        UPDATE,
        SETFILE,
        DELFILE
    }
}
