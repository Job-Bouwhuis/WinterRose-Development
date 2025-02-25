using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.FileServer
{
    public static class DirectoryDataPacker
    {
        public static string PackDirectoryData(string path)
        {
            DirectoryInfo info = new(path);
            StringBuilder builder = new();

            builder.Append("@dirs@");
            foreach (var dir in info.GetDirectories())
            {
                builder.Append(dir.Name);
                builder.Append(',');
                builder.Append(dir.LastWriteTimeUtc.Ticks);
                builder.Append(';');
            }

            builder.Append("@files@");
            foreach (var file in info.GetFiles())
            {
                
                builder.Append(file.Name);
                builder.Append(',');
                builder.Append(file.Length);
                builder.Append(',');
                builder.Append(file.LastWriteTimeUtc.Ticks);
                builder.Append(';');
            }

            return builder.ToString();
        }
    }
}
