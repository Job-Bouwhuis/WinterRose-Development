using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.ForgeMantle.Models;

namespace WinterRose.ForgeMantle;
public interface IUndoBuffer
{
    void Push(ConfigSnapshot snapshot);
    ConfigSnapshot? Undo();
    ConfigSnapshot? Redo();
}

