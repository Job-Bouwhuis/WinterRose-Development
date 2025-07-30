using ForgeMantle.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForgeMantle;
public interface IUndoBuffer
{
    void Push(ConfigSnapshot snapshot);
    ConfigSnapshot? Undo();
    ConfigSnapshot? Redo();
}

