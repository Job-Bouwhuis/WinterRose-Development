using FileChangeTracker.ops;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace FileChangeTracker
{
    public static class FileChangeInstructionParser
    {
        public static List<FileChangeInstruction> ReadBinaryInstructions(Stream input)
        {
            var list = new List<FileChangeInstruction>();
            using var reader = new BinaryReader(input, Encoding.UTF8, leaveOpen: true);

            while (input.Position < input.Length)
            {
                var opcode = (FileChangeOpcode)reader.ReadByte();

                if (opcode == FileChangeOpcode.SETFILE || opcode == FileChangeOpcode.DELFILE)
                {
                    int dataLength = reader.ReadInt32();
                    byte[] data = reader.ReadBytes(dataLength);
                    string filePath = Encoding.UTF8.GetString(data);
                    if (opcode is FileChangeOpcode.SETFILE)
                        list.Add(new SetFileInstruction(filePath));
                    else
                        list.Add(new DeleteFileInstruction(filePath));
                    continue;
                }
                else
                {
                    long offset = reader.ReadInt64();
                    int dataLength = reader.ReadInt32();
                    byte[] data = reader.ReadBytes(dataLength);

                    switch (opcode)
                    {
                        case FileChangeOpcode.INSERT:
                            list.Add(new InsertInstruction(offset, data));
                            break;
                        case FileChangeOpcode.UPDATE:
                            list.Add(new UpdateInstruction(offset, data));
                            break;
                        case FileChangeOpcode.DELETE:
                            list.Add(new DeleteInstruction(offset, BitConverter.ToInt32(data)));
                            break;
                    }
                }
            }


            return list;
        }

        public static void WriteBinaryInstructions(Stream output, List<FileChangeInstruction> instructions)
        {
            using var writer = new BinaryWriter(output, Encoding.UTF8, leaveOpen: true);

            foreach (var instruction in instructions)
            {
                writer.Write((byte)instruction.opcode);
                if (instruction.opcode == FileChangeOpcode.SETFILE || instruction.opcode == FileChangeOpcode.DELFILE)
                {
                    writer.Write(instruction.path!.Length);
                    writer.Write(Encoding.UTF8.GetBytes(instruction.path));
                }
                else
                {
                    long offset = instruction.offset;
                    writer.Write(offset);

                    byte[]? bytes = instruction.data;
                    if (bytes is not null)
                    {
                        writer.Write(bytes.Length);
                        writer.Write(bytes);
                    }
                }
            }
        }

    }
}
