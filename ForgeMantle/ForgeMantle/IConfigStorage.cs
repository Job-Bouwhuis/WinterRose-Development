using ForgeMantle.Models;
using ForgeMantle.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForgeMantle;

public interface IConfigStorage
{
    IConfigSerializer Serializer { get; }
    ConfigSnapshot? Load();
    void Save(ConfigSnapshot snapshot);
}
