using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.ForgeMantle.Models;
using WinterRose.ForgeMantle.Serialization;
using WinterRose.WinterForgeSerializing;

namespace WinterRose.ForgeMantle;

public class FileStorage : IConfigStorage
{
    public IConfigSerializer Serializer { get; }

    private readonly string path;

    public FileStorage(string path, IConfigSerializer serializer)
    {
        this.path = path;
        Serializer = serializer;

        List<string> list = path.Split('/', '\\').ToList();
        if (list.Count > 1)
        {
            list.RemoveAt(list.Count - 1);
            string path2 = string.Join("/", list);
            if (!Directory.Exists(path2))
                Directory.CreateDirectory(path2);
        }

        File.Create(path).Close();
    }

    public ConfigSnapshot? Load()
    {
        using FileStream stream = File.OpenRead(path);
        return Serializer.Deserialize(stream);
    }

    public void Save(ConfigSnapshot snapshot)
    {
        using FileStream stream = File.OpenWrite(path);
        Serializer.Serialize(snapshot, stream);
    }
}

