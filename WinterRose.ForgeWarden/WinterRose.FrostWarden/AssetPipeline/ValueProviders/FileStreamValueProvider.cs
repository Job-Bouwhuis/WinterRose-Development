using System;
using System.Collections.Generic;
using System.Text;
using WinterRose.WinterForgeSerializing;
using WinterRose.WinterForgeSerializing.Workers;

namespace WinterRose.ForgeWarden.AssetPipeline.ValueProviders;

internal class FileStreamValueProvider : CustomValueProvider<FileStream>
{
    public override FileStream? CreateObject(object value, WinterForgeVM executor)
    {
        if (value is not string path)
            throw new ArgumentException("Serialized value was not a path!");

        return File.Open(path, FileMode.OpenOrCreate, FileAccess.ReadWrite);
    }
    public override object CreateString(FileStream obj, ObjectSerializer serializer)
    {
        return obj.Name;
    }
}
