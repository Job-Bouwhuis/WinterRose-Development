using ForgeMantle.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForgeMantle;
public class UndoBuffer : IUndoBuffer
{
    private readonly int maxSize;
    private readonly Stack<ConfigSnapshot> undoStack = new();
    private readonly Stack<ConfigSnapshot> redoStack = new();

    public UndoBuffer(int maxSize = 10)
    {
        this.maxSize = maxSize;
    }

    public void Push(ConfigSnapshot snapshot)
    {
        undoStack.Push(snapshot);
        if (undoStack.Count > maxSize)
            _ = undoStack.Reverse().Skip(maxSize).ToList();

        redoStack.Clear();
    }

    public ConfigSnapshot? Undo()
    {
        if (undoStack.Count == 0)
            return null;

        var snapshot = undoStack.Pop();
        redoStack.Push(snapshot);
        return snapshot;
    }

    public ConfigSnapshot? Redo()
    {
        if (redoStack.Count == 0)
            return null;

        var snapshot = redoStack.Pop();
        undoStack.Push(snapshot);
        return snapshot;
    }
}

