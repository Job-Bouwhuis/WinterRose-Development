using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.ForgeMantle.Models;
using WinterRose.ForgeMantle.Serialization;

namespace WinterRose.ForgeMantle;

public interface IConfigStorage
{
    IConfigSerializer Serializer { get; }
    ConfigSnapshot? Load();
    void Save(ConfigSnapshot snapshot);
}
