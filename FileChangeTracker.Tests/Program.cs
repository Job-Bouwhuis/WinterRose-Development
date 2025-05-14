using FileChangeTracker.detecting;
using FileChangeTracker.ops;
using Octodiff.Core;
using System.Text;

namespace FileChangeTracker.Tests;

internal class Program
{
    static void Main(string[] args)
    {
        File.Copy("original.txt", "myFile.txt", true);

        //using FileStream original = File.Open("every-wound-becomes-a-star.wav", FileMode.Open, FileAccess.Read);
        //using FileStream newFile = File.Open("every-wound-becomes-a-star EDIT.wav", FileMode.Open, FileAccess.Read);
        //var instructions = FileDiffGenerator.Diff(original, newFile, "out.wav");

        using FileStream original = File.Open("original.txt", FileMode.Open, FileAccess.Read);
        using FileStream newFile = File.Open("new.txt", FileMode.Open, FileAccess.Read);
        FileChunkTree origTree = FileChunkTree.BuildFromStream(original);
        FileChunkTree newTree = FileChunkTree.BuildFromStream(newFile);
        var instructions = FileChunkTree.GenerateInstructions(origTree, newTree, original, newFile, "myfile.txt");

        using FileStream instructionFile = File.Open("instr.txt", FileMode.Create, FileAccess.ReadWrite);
        FileChangeInstructionParser.WriteBinaryInstructions(instructionFile, instructions);
        instructionFile.Position = 0;
        var instr = FileChangeInstructionParser.ReadBinaryInstructions(instructionFile);
        FileChangeExecutor.Executie(instr);
    }
}
