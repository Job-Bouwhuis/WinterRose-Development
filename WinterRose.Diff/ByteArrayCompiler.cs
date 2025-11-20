using System;
using System.Collections.Generic;
using System.Text;
using WinterRose.WinterForgeSerializing.Compiling;

namespace WinterRose.Diff;

internal class ByteArrayCompiler : CustomValueCompiler<byte[]>
{
    public override void Compile(BinaryWriter writer, byte[] value)
    {
        writer.Write(value.Length);
        writer.Write(value);
    }

    public override byte[]? Decompile(BinaryReader reader)
    {
        int length = reader.ReadInt32();
        byte[]? result = reader.ReadBytes(length);
        return result.Length == length ? result : null;
    }
}
