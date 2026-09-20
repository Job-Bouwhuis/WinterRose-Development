namespace WinterRose.Diff.Storage;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

public static class DiffSerializer
{
    private const uint MAGIC = 0x46445257; // "WRDF"
    private const byte VERSION = 1;

    private enum OpCode : byte
    {
        Delete = 0,
        DeleteFile = 1,
        Insert = 2,
        Update = 3
    }

    public static void Write(Stream stream, DirectoryDiff diff)
    {
        using BinaryWriter writer = new(stream, Encoding.UTF8, true);

        writer.Write(MAGIC);
        writer.Write(VERSION);

        WriteVarInt(writer, diff.FileDiffs.Count);

        foreach (var pair in diff.FileDiffs)
        {
            WriteString(writer, pair.Key);
            WriteFileDiff(writer, pair.Value);
        }
    }

    public static DirectoryDiff Read(Stream stream)
    {
        using BinaryReader reader = new(stream, Encoding.UTF8, true);

        if (reader.ReadUInt32() != MAGIC)
            throw new InvalidDataException("Invalid diff file.");

        byte version = reader.ReadByte();

        if (version != VERSION)
            throw new InvalidDataException($"Unsupported diff version {version}.");

        int count = ReadVarInt(reader);

        DirectoryDiff result = new();

        for (int i = 0; i < count; i++)
        {
            string path = ReadString(reader);
            result.FileDiffs[path] = ReadFileDiff(reader);
        }

        return result;
    }

    private static void WriteFileDiff(BinaryWriter writer, FileDiff diff)
    {
        writer.Write((byte)diff.State);

        if (diff.NewFileHash == null)
        {
            writer.Write(false);
        }
        else
        {
            writer.Write(true);
            WriteString(writer, diff.NewFileHash);
        }

        WriteVarInt(writer, diff.Operations.Count);

        foreach (Op op in diff.Operations)
            WriteOperation(writer, op);
    }

    private static FileDiff ReadFileDiff(BinaryReader reader)
    {
        FileState state = (FileState)reader.ReadByte();

        string hash = null;

        if (reader.ReadBoolean())
            hash = ReadString(reader);

        int operationCount = ReadVarInt(reader);

        List<Op> operations = new(operationCount);

        for (int i = 0; i < operationCount; i++)
            operations.Add(ReadOperation(reader));

        FileDiff result = new(state, operations)
        {
            NewFileHash = hash
        };

        return result;
    }
    
    private static void WriteOperation(BinaryWriter writer, Op op)
    {
        switch (op)
        {
            case Delete delete:
                writer.Write((byte)OpCode.Delete);
                WriteVarInt(writer, delete.Offset);
                WriteVarInt(writer, delete.Length);
                break;

            case DeleteFile:
                writer.Write((byte)OpCode.DeleteFile);
                break;

            case Insert insert:
                writer.Write((byte)OpCode.Insert);
                WriteVarInt(writer, insert.Offset);
                WriteVarInt(writer, insert.Data.Length);
                writer.Write(insert.Data);
                break;

            case Update update:
                writer.Write((byte)OpCode.Update);
                WriteVarInt(writer, update.Offset);
                WriteVarInt(writer, update.Length);
                WriteVarInt(writer, update.Data.Length);
                writer.Write(update.Data);
                break;

            default:
                throw new InvalidOperationException(
                    $"Unknown operation type {op.GetType()}");
        }
    }

    private static Op ReadOperation(BinaryReader reader)
    {
        OpCode code = (OpCode)reader.ReadByte();

        return code switch
        {
            OpCode.Delete =>
                new Delete(
                    ReadVarInt64(reader),
                    ReadVarInt64(reader)),

            OpCode.DeleteFile =>
                new DeleteFile(),

            OpCode.Insert =>
                new Insert(
                    ReadVarInt64(reader),
                    reader.ReadBytes(ReadVarInt(reader))),

            OpCode.Update =>
                new Update(
                    ReadVarInt64(reader),
                    ReadVarInt64(reader),
                    reader.ReadBytes(ReadVarInt(reader))),

            _ => throw new InvalidDataException($"Unknown opcode {code}")
        };
    }
    
    private static void WriteString(BinaryWriter writer, string value)
    {
        byte[] data = Encoding.UTF8.GetBytes(value);

        WriteVarInt(writer, data.Length);
        writer.Write(data);
    }

    private static string ReadString(BinaryReader reader)
    {
        int length = ReadVarInt(reader);
        return Encoding.UTF8.GetString(reader.ReadBytes(length));
    }

    private static void WriteVarInt(BinaryWriter writer, long value)
    {
        ulong number = (ulong)((value << 1) ^ (value >> 63));

        while (number >= 0x80)
        {
            writer.Write((byte)(number | 0x80));
            number >>= 7;
        }

        writer.Write((byte)number);
    }

    private static long ReadVarInt64(BinaryReader reader)
    {
        ulong result = 0;
        int shift = 0;

        while (true)
        {
            byte current = reader.ReadByte();

            result |= (ulong)(current & 0x7F) << shift;

            if ((current & 0x80) == 0)
                break;

            shift += 7;
        }

        return (long)((result >> 1) ^ (ulong)-(long)(result & 1));
    }

    private static int ReadVarInt(BinaryReader reader)
    {
        return checked((int)ReadVarInt64(reader));
    }
}