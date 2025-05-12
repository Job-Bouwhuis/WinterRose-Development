using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileChangeTracker
{
    public static class FileChangeExecutor
    {
        public static void Executie(List<FileChangeInstruction> instructions)
        {
            using FileChangeContext context = new();
            foreach (var instr in instructions)
            {
                //Console.WriteLine(instr.ToString());
                instr.Execute(context);
            }
        }
    }
}
